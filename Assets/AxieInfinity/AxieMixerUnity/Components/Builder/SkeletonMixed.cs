using System;
using System.Collections.Generic;
using Spine;
using AxieCore.AxieMixer;
using System.Globalization;

namespace AxieMixer.Unity
{
	public class SkeletonMixed
	{
		public float Scale { get; set; }

		private AttachmentLoader attachmentLoader;
		private List<LinkedMesh> linkedMeshes = new List<LinkedMesh>();

		public SkeletonMixed(params Atlas[] atlasArray)
			: this(new AtlasAttachmentLoader(atlasArray))
		{
		}

		public SkeletonMixed(AttachmentLoader attachmentLoader)
		{
			if (attachmentLoader == null) throw new ArgumentNullException("attachmentLoader", "attachmentLoader cannot be null.");
			this.attachmentLoader = attachmentLoader;
			Scale = 1;
		}

		public SkeletonData ReadSkeletonData(MixedSkeletonData jData, bool logError = false)
		{
			if (jData == null) throw new ArgumentNullException("reader", "reader cannot be null.");
			float scale = this.Scale;
			var skeletonData = new SkeletonData();

			string spineName = "";
			if (!string.IsNullOrEmpty(jData.name))
			{
				spineName = jData.name;
			}

			// Bones.

			foreach (var jBone in jData.bones)
			{
				BoneData parent = null;
				if (!string.IsNullOrEmpty(jBone.parent))
				{
					parent = skeletonData.FindBone(jBone.parent);
					if (parent == null)
						throw new Exception("Parent bone not found: " + jBone.parent);
				}
				var data = new BoneData(skeletonData.Bones.Count, jBone.name, parent);
				data.Length = GetFloat(jBone.length, 0) * scale;
				data.X = GetFloat(jBone.x, 0) * scale;
				data.Y = GetFloat(jBone.y, 0) * scale;

				data.Rotation = GetFloat(jBone.rotation, 0);
				data.ScaleX = GetFloat(jBone.scaleX, 1);// GetFloat(boneMap, "scaleX", 1);
				data.ScaleY = GetFloat(jBone.scaleX, 1);//GetFloat(boneMap, "scaleY", 1);
				data.ShearX = 0.0f;//GetFloat(boneMap, "shearX", 0);
				data.ShearY = 0.0f;//GetFloat(boneMap, "shearY", 0);

				string tm = GetString(jBone.transform, TransformMode.Normal.ToString());
				data.TransformMode = (TransformMode)Enum.Parse(typeof(TransformMode), tm, true);
				data.SkinRequired = GetBoolean(jBone.skin, false);

				skeletonData.Bones.Add(data);
			}

			// Slots.

			foreach (var jSlot in jData.slots)
			{
				var slotName = jSlot.name;
				var boneName = jSlot.bone;
				BoneData boneData = skeletonData.FindBone(boneName);
				if (boneData == null) throw new Exception("Slot bone not found: " + boneName);
				var data = new SlotData(skeletonData.Slots.Count, slotName, boneData);

				data.R = jSlot.colorVariant / 255.0f;
				data.G = jSlot.colorShift / 255.0f;
				if (!string.IsNullOrEmpty(jSlot.color))
                {
					data.A = ToColor(jSlot.color, 3);
				}
				//if (slotMap.ContainsKey("color"))
				//{
				//	string color = (string)slotMap["color"];
				//	data.r = ToColor(color, 0);
				//	data.g = ToColor(color, 1);
				//	data.b = ToColor(color, 2);
				//	data.a = ToColor(color, 3);
				//}

				//if (slotMap.ContainsKey("dark"))
				//{
				//	var color2 = (string)slotMap["dark"];
				//	data.r2 = ToColor(color2, 0, 6); // expectedLength = 6. ie. "RRGGBB"
				//	data.g2 = ToColor(color2, 1, 6);
				//	data.b2 = ToColor(color2, 2, 6);
				//	data.hasSecondColor = true;
				//}

				data.AttachmentName = GetString(jSlot.attachment, null);

				if (!string.IsNullOrEmpty(jSlot.blendMode))
				{
					data.BlendMode = (BlendMode)Enum.Parse(typeof(BlendMode), jSlot.blendMode, true);
				}
				else
				{
					data.BlendMode = BlendMode.Normal;
				}
				skeletonData.Slots.Add(data);
			}
		
			// IK constraints.
			
			foreach (var entry in jData.ik)
			{
				IkConstraintData data = new IkConstraintData(entry.name);
				data.Order = GetInt(entry.order, 0);
				data.SkinRequired = false;// GetBoolean(constraintMap, "skin", false);

				if (entry.bones != null)
				{
					foreach (string boneName in entry.bones)
					{
						BoneData bone = skeletonData.FindBone(boneName);
						if (bone == null) throw new Exception("IK bone not found: " + boneName);
						data.Bones.Add(bone);
					}
				}

				string targetName = entry.target;
				data.Target = skeletonData.FindBone(targetName);
				if (data.Target == null) throw new Exception("IK target bone not found: " + targetName);

				data.Mix = 1;// GetFloat(constraintMap, "mix", 1);
				data.Softness = 0;// GetFloat(constraintMap, "softness", 0) * scale;
				data.BendDirection = 1;// GetBoolean(constraintMap, "bendPositive", true) ? 1 : -1;
				data.Compress = false;// GetBoolean(constraintMap, "compress", false);
				data.Stretch = false;// GetBoolean(constraintMap, "stretch", false);
				data.Uniform = false;// GetBoolean(constraintMap, "uniform", false);

				skeletonData.IkConstraints.Add(data);
			}

			// Transform constraints.
			//if (root.ContainsKey("transform"))
			//{
			//}

			// Path constraints.
			//if (root.ContainsKey("path"))
			//{
			//}

			// Skins.
			
			foreach (var jSkin in jData.skins)
			{
				Skin skin = new Skin(jSkin.name);
				//if (skinMap.ContainsKey("bones"))
				//{
				//}
				//if (skinMap.ContainsKey("ik"))
				//{
				//}
				//if (skinMap.ContainsKey("transform"))
				//{
				//}
				//if (skinMap.ContainsKey("path"))
				//{
				//}
				if (jSkin.attachments != null)
				{
					foreach (var slotEntry in jSkin.attachments)
					{
						int slotIndex = skeletonData.FindSlotIndex(slotEntry.Key);
						foreach (var entry in slotEntry.Value)
						{
							try
							{
								Attachment attachment = ReadAttachment(entry.Value, skin, slotIndex, entry.Key, skeletonData);
								if (attachment != null) skin.SetAttachment(slotIndex, entry.Key, attachment);
							}
							catch (Exception)
							{
								if (logError)
								{
									UnityEngine.Debug.LogWarning($"Error reading attachment: {entry.Key} {entry.Value.path}, skin: {skin}, slotEntry: {slotEntry.Key}, spineName: {spineName}");
								}
							}
						}
					}
				}
				skeletonData.Skins.Add(skin);
				if (skin.Name == "default") skeletonData.DefaultSkin = skin;
			}
			

			// Linked meshes.
			for (int i = 0, n = linkedMeshes.Count;i < n;i++)
			{
				LinkedMesh linkedMesh = linkedMeshes[i];
				Skin skin = linkedMesh.skin == null ? skeletonData.DefaultSkin : skeletonData.FindSkin(linkedMesh.skin);
				if (skin == null) throw new Exception("Slot not found: " + linkedMesh.skin);
				Attachment parent = skin.GetAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent == null) throw new Exception("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.DeformAttachment = linkedMesh.inheritDeform ? (VertexAttachment)parent : linkedMesh.mesh;
				linkedMesh.mesh.ParentMesh = (MeshAttachment)parent;
				linkedMesh.mesh.UpdateUVs();
			}
			linkedMeshes.Clear();

            // Events.
            if (jData.events != null)
            {
                foreach (var entry in jData.events)
                {
                    var data = new EventData(entry.Key);
                    skeletonData.Events.Add(data);
                }
            }

            // Animations.

            foreach (var entry in jData.animations)
			{
				try
				{
					ReadAnimation(entry.Value, entry.Key, skeletonData);
				}
				catch (Exception e)
				{
					throw new Exception("Error reading animation: " + entry.Key, e);
				}
			}
			
			skeletonData.Bones.TrimExcess();
			skeletonData.Slots.TrimExcess();
			skeletonData.Skins.TrimExcess();
			skeletonData.Events.TrimExcess();
			skeletonData.Animations.TrimExcess();
			skeletonData.IkConstraints.TrimExcess();
			return skeletonData;
		}

		private Attachment ReadAttachment(SampleSkinAttachmentData map, Skin skin, int slotIndex, string name, SkeletonData skeletonData)
		{
			float scale = this.Scale;

			var typeName = GetString(map.type, "region");
			var type = (AttachmentType)Enum.Parse(typeof(AttachmentType), typeName, true);

			string path = GetString(map.path, name);

			switch (type)
			{
				case AttachmentType.Region:
					RegionAttachment region = attachmentLoader.NewRegionAttachment(skin, name, path);
					if (region == null) return null;
					region.Path = path;
					region.X = GetFloat(map.x, 0) * scale;
					region.Y = GetFloat(map.y, 0) * scale;
					region.ScaleX = 1;// GetFloat(map.sx, 1);
					region.ScaleY = 1;// GetFloat(map, "scaleY", 1);
					region.Rotation = GetFloat(map.rotation, 0);
					region.Width = GetFloat(map.width, 32) * scale;
					region.Height = GetFloat(map.height, 32) * scale;

					region.UpdateOffset();
					return region;
				case AttachmentType.Mesh:
					{
						MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path);
						if (mesh == null) return null;
						mesh.Path = path;
					
						mesh.Width = GetFloat(map.width, 0) * scale;
						mesh.Height = GetFloat(map.height, 0) * scale;

						//string parent = GetString(map, "parent", null);
						//if (parent != null)
						//{
						//	linkedMeshes.Add(new LinkedMesh(mesh, GetString(map, "skin", null), slotIndex, parent, GetBoolean(map, "deform", true)));
						//	return mesh;
						//}

						float[] uvs = GetFloatArray(map.uvs, 1);
						ReadVertices(map.vertices, mesh, uvs.Length);
						mesh.Triangles = map.triangles.ToArray();// GetIntArray(map, "triangles");
						mesh.RegionUVs = uvs;
						mesh.UpdateUVs();

						if (map.hull.HasValue) mesh.HullLength = GetInt(map.hull, 0) * 2;
						if (map.edges != null) mesh.Edges = map.edges.ToArray();// GetIntArray(map, "edges");
						return mesh;
					}
				case AttachmentType.Clipping:
					{
						ClippingAttachment clip = attachmentLoader.NewClippingAttachment(skin, name);
						if (clip == null) return null;

						if (!string.IsNullOrEmpty( map.end))
						{
							SlotData slot = skeletonData.FindSlot(map.end);
							if (slot == null) throw new Exception("Clipping end slot not found: " + map.end);
							clip.EndSlot = slot;
						}
						ReadVertices(map.vertices, clip, GetInt(map.vertexCount, 0) << 1);
						return clip;
					}

			}
			return null;
		}

		private void ReadVertices(List<float> vertices, VertexAttachment attachment, int verticesLength)
		{
			attachment.WorldVerticesLength = verticesLength;
			float scale = Scale;
			if (verticesLength == vertices.Count)
			{
				var finalVertices = vertices.ToArray();
				if (scale != 1)
				{
					for (int i = 0;i < vertices.Count;i++)
					{
						finalVertices[i] *= scale;
					}
				}
				attachment.Vertices = finalVertices;
				return;
			}
			ExposedList<float> weights = new ExposedList<float>(verticesLength * 3 * 3);
			ExposedList<int> bones = new ExposedList<int>(verticesLength * 3);
			for (int i = 0, n = vertices.Count;i < n;)
			{
				int boneCount = (int)vertices[i++];
				bones.Add(boneCount);
				for (int nn = i + boneCount * 4;i < nn;i += 4)
				{
					bones.Add((int)vertices[i]);
					weights.Add(vertices[i + 1] * this.Scale);
					weights.Add(vertices[i + 2] * this.Scale);
					weights.Add(vertices[i + 3]);
				}
			}
			attachment.Bones = bones.ToArray();
			attachment.Vertices = weights.ToArray();
		}

		private void ReadAnimation(SampleAnimationData map, string name, SkeletonData skeletonData)
		{
			var scale = this.Scale;
			var timelines = new ExposedList<Timeline>();
			float duration = 0;

			// Slot timelines.
			if (map.slots != null)
			{
				foreach (var entry in map.slots)
				{
					string slotName = entry.Key;
					int slotIndex = skeletonData.FindSlotIndex(slotName);
					var timelineMap = entry.Value;

					if (timelineMap.attachment != null)
					{
						var values = timelineMap.attachment;
						var timeline = new AttachmentTimeline(values.Count);
						timeline.SlotIndex = slotIndex;

						int frameIndex = 0;
						foreach (var valueMap in values)
						{
							float time = GetFloat(valueMap.time, 0);
							timeline.SetFrame(frameIndex++, time, valueMap.name);
						}
						timelines.Add(timeline);
						duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);

					}
					if (timelineMap.color != null)
					{
						var values = timelineMap.color;
						var timeline = new ColorTimeline(values.Count);
						timeline.SlotIndex = slotIndex;

						int frameIndex = 0;
						foreach (var valueMap in values)
						{
							float time = GetFloat(valueMap.time, 0);
							string c = valueMap.color;
							timeline.SetFrame(frameIndex, time, ToColor(c, 0), ToColor(c, 1), ToColor(c, 2), ToColor(c, 3));
							ReadCurve(valueMap, timeline, frameIndex);
							frameIndex++;
						}
						timelines.Add(timeline);
						duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * ColorTimeline.ENTRIES]);
					}
				}
			}

			// Bone timelines.
			if (map.bones != null)
			{
				foreach (var entry in map.bones)
				{
					string boneName = entry.Key;
					int boneIndex = skeletonData.FindBoneIndex(boneName);
					if (boneIndex == -1) throw new Exception("Bone not found: " + boneName);
					var timelineMap = entry.Value;

					if (timelineMap.rotate != null)
					{
						var values = timelineMap.rotate;
						var timeline = new RotateTimeline(values.Count);
						timeline.BoneIndex = boneIndex;

						int frameIndex = 0;
						foreach (var valueMap in values)
						{
							timeline.SetFrame(frameIndex, GetFloat(valueMap.time, 0), GetFloat(valueMap.angle, 0));
							ReadCurve(valueMap, timeline, frameIndex);
							frameIndex++;
						}
						timelines.Add(timeline);
						duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * RotateTimeline.ENTRIES]);

					}
					if (timelineMap.translate != null)
					{
						var values = timelineMap.translate;
						TranslateTimeline timeline = new TranslateTimeline(values.Count);
						float timelineScale = scale;
						float defaultValue = 0;

						timeline.BoneIndex = boneIndex;

						int frameIndex = 0;
						foreach (var valueMap in values)
						{
							float time = GetFloat(valueMap.time, 0);
							float x = GetFloat(valueMap.x, defaultValue);
							float y = GetFloat(valueMap.y, defaultValue);
							timeline.SetFrame(frameIndex, time, x * timelineScale, y * timelineScale);
							ReadCurve(valueMap, timeline, frameIndex);
							frameIndex++;
						}
						timelines.Add(timeline);
						duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * TranslateTimeline.ENTRIES]);
					}

					if (timelineMap.scale != null)
					{
						var values = timelineMap.scale;
						TranslateTimeline timeline = new ScaleTimeline(values.Count);
						float timelineScale = 1;
						float defaultValue = 1;

						timeline.BoneIndex = boneIndex;

						int frameIndex = 0;
						foreach (var valueMap in values)
						{
							float time = GetFloat(valueMap.time, 0);
							float x = GetFloat(valueMap.x, defaultValue);
							float y = GetFloat(valueMap.y, defaultValue);
							timeline.SetFrame(frameIndex, time, x * timelineScale, y * timelineScale);
							ReadCurve(valueMap, timeline, frameIndex);
							frameIndex++;
						}
						timelines.Add(timeline);
						duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * TranslateTimeline.ENTRIES]);
					}

					if (timelineMap.shear != null)
					{
						var values = timelineMap.shear;
						TranslateTimeline timeline = new ShearTimeline(values.Count);
						float timelineScale = 1;
						float defaultValue = 0;

						timeline.BoneIndex = boneIndex;

						int frameIndex = 0;
						foreach (var valueMap in values)
						{
							float time = GetFloat(valueMap.time, 0);
							float x = GetFloat(valueMap.x, defaultValue);
							float y = GetFloat(valueMap.y, defaultValue);
							timeline.SetFrame(frameIndex, time, x * timelineScale, y * timelineScale);
							ReadCurve(valueMap, timeline, frameIndex);
							frameIndex++;
						}
						timelines.Add(timeline);
						duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * TranslateTimeline.ENTRIES]);
					}
				}
			}

			// IK constraint timelines.
			//if (map.ContainsKey("ik"))
			//{
			//}

			// Transform constraint timelines.
			//if (map.ContainsKey("transform"))
			//{
			//}

			// Path constraint timelines.
			//if (map.ContainsKey("path"))
			//{
			//}

			// Deform timelines.
			//if (map.ContainsKey("deform"))
			//{
			//}

			// Draw order timeline.
			if (map.drawOrder != null)
			{
				var values = map.drawOrder;
				var timeline = new DrawOrderTimeline(values.Count);
				int slotCount = skeletonData.Slots.Count;
				int frameIndex = 0;
				foreach (var drawOrderMap in values)
				{
					int[] drawOrder = null;
					if (drawOrderMap.offsets != null)
					{
						drawOrder = new int[slotCount];
						for (int i = slotCount - 1;i >= 0;i--)
							drawOrder[i] = -1;
						var offsets = drawOrderMap.offsets;
						int[] unchanged = new int[slotCount - offsets.Count];
						int originalIndex = 0, unchangedIndex = 0;
						foreach (var offsetMap in offsets)
						{
							int slotIndex = skeletonData.FindSlotIndex(offsetMap.slot);
							if (slotIndex == -1)
								throw new Exception("Slot not found: " + offsetMap.slot);
							// Collect unchanged items.
							while (originalIndex != slotIndex)
								unchanged[unchangedIndex++] = originalIndex++;
							// Set changed items.
							int index = originalIndex + offsetMap.offset;
							while (drawOrder[index] != -1)
                            {
								index++;
							}
							drawOrder[index] = originalIndex++;
						}
						// Collect remaining unchanged items.
						while (originalIndex < slotCount)
							unchanged[unchangedIndex++] = originalIndex++;
						// Fill in unchanged items.
						for (int i = slotCount - 1;i >= 0;i--)
						{
							if(unchangedIndex == 0)
                            {
								int aaa = 0;
								aaa++;
                            }
							if (drawOrder[i] == -1)
								drawOrder[i] = unchanged[--unchangedIndex];
						}
						
					}
					timeline.SetFrame(frameIndex++, GetFloat(drawOrderMap.time, 0), drawOrder);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
			}

            // Event timeline.
            if (map.events != null)
            {
				var eventsMap = map.events;
                var timeline = new EventTimeline(eventsMap.Count);
                int frameIndex = 0;
                foreach (var eventMap in eventsMap)
                {
                    EventData eventData = skeletonData.FindEvent(eventMap.name);
                    if (eventData == null) throw new Exception("Event not found: " + eventMap.name);
					var e = new Event(GetFloat(eventMap.time, 0), eventData);
                    timeline.SetFrame(frameIndex++, e);
                }
                timelines.Add(timeline);
                duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
            }

            timelines.TrimExcess();
			skeletonData.Animations.Add(new Animation(name, timelines, duration));
		}

		static void ReadCurve(SampleBaseAnimationTimeData valueMap, CurveTimeline timeline, int frameIndex)
		{
			if (string.IsNullOrEmpty(valueMap.curve))
				return;
			var curveObject = valueMap.curve;
			if (float.TryParse((string)curveObject, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
				{
				timeline.SetCurve(frameIndex, val, GetFloat(valueMap.c2, 0), GetFloat(valueMap.c3, 1), GetFloat(valueMap.c4, 1));
			}
			else
			{
				timeline.SetStepped(frameIndex);
			}
		}

		internal class LinkedMesh
		{
			internal string parent, skin;
			internal int slotIndex;
			internal MeshAttachment mesh;
			internal bool inheritDeform;

			public LinkedMesh(MeshAttachment mesh, string skin, int slotIndex, string parent, bool inheritDeform)
			{
				this.mesh = mesh;
				this.skin = skin;
				this.slotIndex = slotIndex;
				this.parent = parent;
				this.inheritDeform = inheritDeform;
			}
		}

		static float[] GetFloatArray(List<float> list, float scale)
		{
			var values = new float[list.Count];
			if (scale == 1)
			{
				for (int i = 0, n = list.Count;i < n;i++)
					values[i] = (float)list[i];
			}
			else
			{
				for (int i = 0, n = list.Count;i < n;i++)
					values[i] = (float)list[i] * scale;
			}
			return values;
		}

		static float GetFloat(float? val, float defaultValue)
		{
			if (val == null)
				return defaultValue;
			return (float)val.Value;
		}

		static int GetInt(int? val, int defaultValue)
		{
			if (!val.HasValue)
				return defaultValue;
			return (int)val;
		}

		static bool GetBoolean(bool? val, bool defaultValue)
		{
			if (!val.HasValue)
				return defaultValue;
			return (bool)val.Value;
		}

		static string GetString(string val, string defaultValue)
		{
			if (string.IsNullOrEmpty(val))
				return defaultValue;
			return (string)val;
		}

		static float ToColor(string hexString, int colorIndex, int expectedLength = 8)
		{
			if (hexString.Length != expectedLength)
				throw new ArgumentException("Color hexidecimal length must be " + expectedLength + ", recieved: " + hexString, "hexString");
			return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
		}
	}
}
