using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Spine;

namespace AxieMixer.Unity
{
	public class LiteSkeletonJson
	{
		public float Scale { get; set; }

		private AttachmentLoader attachmentLoader;
		private List<LinkedMesh> linkedMeshes = new List<LinkedMesh>();

		public LiteSkeletonJson(params Atlas[] atlasArray)
			: this(new AtlasAttachmentLoader(atlasArray))
		{
		}

		public LiteSkeletonJson(AttachmentLoader attachmentLoader)
		{
			if (attachmentLoader == null) throw new ArgumentNullException("attachmentLoader", "attachmentLoader cannot be null.");
			this.attachmentLoader = attachmentLoader;
			Scale = 1;
		}


		public SkeletonData ReadSkeletonData(JObject jData, bool logError = false)
		{
			if (jData == null) throw new ArgumentNullException("reader", "reader cannot be null.");
			float scale = this.Scale;
			var skeletonData = new SkeletonData();

			var root = jData.ToObject<Dictionary<string, JToken>>();
			if (root == null) throw new Exception("Invalid JSON.");

			string spineName = "";
			if (root.ContainsKey("name"))
			{
				spineName = (string)root["name"];
			}

			// Skeleton.
			if (root.ContainsKey("skeleton"))
			{
				var skeletonMap = root["skeleton"] as JObject;
				skeletonData.Hash = (string)skeletonMap["hash"];
				skeletonData.Version = (string)skeletonMap["spine"];
				if ("3.8.75" == skeletonData.Version)
					throw new Exception("Unsupported skeleton data, please export with a newer version of Spine.");
				skeletonData.X = GetFloat(skeletonMap, "x", 0);
				skeletonData.Y = GetFloat(skeletonMap, "y", 0);
				skeletonData.Width = GetFloat(skeletonMap, "width", 0);
				skeletonData.Height = GetFloat(skeletonMap, "height", 0);
				skeletonData.Fps = GetFloat(skeletonMap, "fps", 30);
				skeletonData.ImagesPath = GetString(skeletonMap, "images", null);
				skeletonData.AudioPath = GetString(skeletonMap, "audio", null);
			}

			// Bones.
			if (root.ContainsKey("bones"))
			{
				var jBones = root["bones"] as JArray;
				foreach (var jBone in jBones)
				{
					//foreach (Dictionary<string, JToken> boneMap in (root["bones"] as JArray) {
					var boneMap = jBone as JObject;
					BoneData parent = null;
					if (boneMap.ContainsKey("parent"))
					{
						parent = skeletonData.FindBone((string)boneMap["parent"]);
						if (parent == null)
							throw new Exception("Parent bone not found: " + boneMap["parent"]);
					}
					var data = new BoneData(skeletonData.Bones.Count, (string)boneMap["name"], parent);
					data.Length = GetFloat(boneMap, "length", 0) * scale;
					data.X = GetFloat(boneMap, "x", 0) * scale;
					data.Y = GetFloat(boneMap, "y", 0) * scale;
					data.Rotation = GetFloat(boneMap, "rotation", 0);
					data.ScaleX = GetFloat(boneMap, "scaleX", 1);
					data.ScaleY = GetFloat(boneMap, "scaleY", 1);
					data.ShearX = GetFloat(boneMap, "shearX", 0);
					data.ShearY = GetFloat(boneMap, "shearY", 0);

					string tm = GetString(boneMap, "transform", TransformMode.Normal.ToString());
					data.TransformMode = (TransformMode)Enum.Parse(typeof(TransformMode), tm, true);
					data.SkinRequired = GetBoolean(boneMap, "skin", false);

					skeletonData.Bones.Add(data);
				}
			}

			// Slots.
			if (root.ContainsKey("slots"))
			{
				var jSlots = root["slots"] as JArray;
				foreach (var jSlot in jSlots)
				{
					var slotMap = jSlot as JObject;
					var slotName = (string)slotMap["name"];
					var boneName = (string)slotMap["bone"];
					BoneData boneData = skeletonData.FindBone(boneName);
					if (boneData == null) throw new Exception("Slot bone not found: " + boneName);
					var data = new SlotData(skeletonData.Slots.Count, slotName, boneData);

					data.R = GetFloat(slotMap, "colorVariant", 0f) / 255.0f;
					data.G = GetFloat(slotMap, "colorShift", 0f) / 255.0f;

					if (slotMap.ContainsKey("color"))
					{
						string color = (string)slotMap["color"];
						//data.R = ToColor(color, 0);
						//data.G = ToColor(color, 1);
						//data.B = ToColor(color, 2);
						data.A = ToColor(color, 3);
					}

					if (slotMap.ContainsKey("dark"))
					{
						var color2 = (string)slotMap["dark"];
						data.R2 = ToColor(color2, 0, 6); // expectedLength = 6. ie. "RRGGBB"
						data.G2 = ToColor(color2, 1, 6);
						data.B2 = ToColor(color2, 2, 6);
						data.HasSecondColor = true;
					}

					data.AttachmentName = GetString(slotMap, "attachment", null);
					if (slotMap.ContainsKey("blend"))
						data.BlendMode = (BlendMode)Enum.Parse(typeof(BlendMode), (string)slotMap["blend"], true);
					else
						data.BlendMode = BlendMode.Normal;
					skeletonData.Slots.Add(data);
				}
			}

			// IK constraints.
			if (root.ContainsKey("ik"))
			{
				foreach (var entry in (root["ik"] as JArray))
				{
					var constraintMap = entry as JObject;
					IkConstraintData data = new IkConstraintData((string)constraintMap["name"]);
					data.Order = GetInt(constraintMap as JObject, "order", 0);
					data.SkinRequired = GetBoolean(constraintMap, "skin", false);

					if (constraintMap.ContainsKey("bones"))
					{
						foreach (string boneName in (constraintMap["bones"] as JArray))
						{
							BoneData bone = skeletonData.FindBone(boneName);
							if (bone == null) throw new Exception("IK bone not found: " + boneName);
							data.Bones.Add(bone);
						}
					}

					string targetName = (string)constraintMap["target"];
					data.Target = skeletonData.FindBone(targetName);
					if (data.Target == null) throw new Exception("IK target bone not found: " + targetName);
					data.Mix = GetFloat(constraintMap, "mix", 1);
					data.Softness = GetFloat(constraintMap, "softness", 0) * scale;
					data.BendDirection = GetBoolean(constraintMap, "bendPositive", true) ? 1 : -1;
					data.Compress = GetBoolean(constraintMap, "compress", false);
					data.Stretch = GetBoolean(constraintMap, "stretch", false);
					data.Uniform = GetBoolean(constraintMap, "uniform", false);

					skeletonData.IkConstraints.Add(data);
				}
			}

			// Transform constraints.
			if (root.ContainsKey("transform"))
			{
				foreach (var entry in (root["transform"] as JArray))
				{
					var constraintMap = entry as JObject;
					TransformConstraintData data = new TransformConstraintData((string)constraintMap["name"]);
					data.Order = GetInt(constraintMap, "order", 0);
					data.SkinRequired = GetBoolean(constraintMap, "skin", false);

					if (constraintMap.ContainsKey("bones"))
					{
						foreach (string boneName in (constraintMap["bones"] as JArray))
						{
							BoneData bone = skeletonData.FindBone(boneName);
							if (bone == null) throw new Exception("Transform constraint bone not found: " + boneName);
							data.Bones.Add(bone);
						}
					}

					string targetName = (string)constraintMap["target"];
					data.Target = skeletonData.FindBone(targetName);
					if (data.Target == null) throw new Exception("Transform constraint target bone not found: " + targetName);

					data.Local = GetBoolean(constraintMap, "local", false);
					data.Relative = GetBoolean(constraintMap, "relative", false);

					data.OffsetRotation = GetFloat(constraintMap, "rotation", 0);
					data.OffsetX = GetFloat(constraintMap, "x", 0) * scale;
					data.OffsetY = GetFloat(constraintMap, "y", 0) * scale;
					data.OffsetScaleX = GetFloat(constraintMap, "scaleX", 0);
					data.OffsetScaleY = GetFloat(constraintMap, "scaleY", 0);
					data.OffsetShearY = GetFloat(constraintMap, "shearY", 0);

					data.RotateMix = GetFloat(constraintMap, "rotateMix", 1);
					data.TranslateMix = GetFloat(constraintMap, "translateMix", 1);
					data.ScaleMix = GetFloat(constraintMap, "scaleMix", 1);
					data.ShearMix = GetFloat(constraintMap, "shearMix", 1);

					skeletonData.TransformConstraints.Add(data);
				}
			}

			// Path constraints.
			if (root.ContainsKey("path"))
			{
				foreach (var entry in (root["path"] as JArray))
				{
					var constraintMap = entry as JObject;
					PathConstraintData data = new PathConstraintData((string)constraintMap["name"]);
					data.Order = GetInt(constraintMap, "order", 0);
					data.SkinRequired = GetBoolean(constraintMap, "skin", false);

					if (constraintMap.ContainsKey("bones"))
					{
						foreach (string boneName in (constraintMap["bones"] as JArray))
						{
							BoneData bone = skeletonData.FindBone(boneName);
							if (bone == null) throw new Exception("Path bone not found: " + boneName);
							data.Bones.Add(bone);
						}
					}

					string targetName = (string)constraintMap["target"];
					data.Target = skeletonData.FindSlot(targetName);
					if (data.Target == null) throw new Exception("Path target slot not found: " + targetName);

					data.PositionMode = (PositionMode)Enum.Parse(typeof(PositionMode), GetString(constraintMap, "positionMode", "percent"), true);
					data.SpacingMode = (SpacingMode)Enum.Parse(typeof(SpacingMode), GetString(constraintMap, "spacingMode", "length"), true);
					data.RotateMode = (RotateMode)Enum.Parse(typeof(RotateMode), GetString(constraintMap, "rotateMode", "tangent"), true);
					data.OffsetRotation = GetFloat(constraintMap, "rotation", 0);
					data.Position = GetFloat(constraintMap, "position", 0);
					if (data.PositionMode == PositionMode.Fixed) data.Position *= scale;
					data.Spacing = GetFloat(constraintMap, "spacing", 0);
					if (data.SpacingMode == SpacingMode.Length || data.SpacingMode == SpacingMode.Fixed) data.Spacing *= scale;
					data.RotateMix = GetFloat(constraintMap, "rotateMix", 1);
					data.TranslateMix = GetFloat(constraintMap, "translateMix", 1);

					skeletonData.PathConstraints.Add(data);
				}
			}

			// Skins.
			if (root.ContainsKey("skins"))
			{
				var jSkins = root["skins"] as JArray;
				foreach (var jSkin in jSkins)
				{
					var skinMap = jSkin as JObject;
					Skin skin = new Skin((string)skinMap["name"]);
					if (skinMap.ContainsKey("bones"))
					{
						foreach (string entryName in (skinMap["bones"] as JArray))
						{
							BoneData bone = skeletonData.FindBone(entryName);
							if (bone == null) throw new Exception("Skin bone not found: " + entryName);
							skin.Bones.Add(bone);
						}
					}
					if (skinMap.ContainsKey("ik"))
					{
						foreach (string entryName in (skinMap["ik"] as JArray))
						{
							IkConstraintData constraint = skeletonData.FindIkConstraint(entryName);
							if (constraint == null) throw new Exception("Skin IK constraint not found: " + entryName);
							skin.Constraints.Add(constraint);
						}
					}
					if (skinMap.ContainsKey("transform"))
					{
						foreach (string entryName in (skinMap["transform"] as JArray))
						{
							TransformConstraintData constraint = skeletonData.FindTransformConstraint(entryName);
							if (constraint == null) throw new Exception("Skin transform constraint not found: " + entryName);
							skin.Constraints.Add(constraint);
						}
					}
					if (skinMap.ContainsKey("path"))
					{
						foreach (string entryName in (skinMap["path"] as JArray))
						{
							PathConstraintData constraint = skeletonData.FindPathConstraint(entryName);
							if (constraint == null) throw new Exception("Skin path constraint not found: " + entryName);
							skin.Constraints.Add(constraint);
						}
					}
					if (skinMap.ContainsKey("attachments"))
					{
						foreach (var slotEntry in (skinMap["attachments"] as JObject))
						{
							int slotIndex = skeletonData.FindSlotIndex(slotEntry.Key);
							foreach (var entry in (slotEntry.Value as JObject))
							{
								try
								{
									Attachment attachment = ReadAttachment(entry.Value as JObject, skin, slotIndex, entry.Key, skeletonData);
									if (attachment != null) skin.SetAttachment(slotIndex, entry.Key, attachment);
								}
								catch (Exception e)
								{
									if (logError)
									{
										UnityEngine.Debug.LogWarning($"Error reading attachment: {entry.Key}, skin: {skin}, spineName: {spineName}");
										//throw new Exception("Error reading attachment: " + entry.Key + ", skin: " + skin, e);
									}
								}
							}
						}
					}
					skeletonData.Skins.Add(skin);
					if (skin.Name == "default") skeletonData.DefaultSkin = skin;
				}
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
			if (root.ContainsKey("events"))
			{
				var jEvents = root["events"] as JObject;
				foreach (var entry in jEvents)
				{
					var entryMap = entry.Value as JObject;
					var data = new EventData(entry.Key);
					data.Int = GetInt(entryMap, "int", 0);
					data.Float = GetFloat(entryMap, "float", 0);
					data.String = GetString(entryMap, "string", string.Empty);
					data.AudioPath = GetString(entryMap, "audio", null);
					if (data.AudioPath != null)
					{
						data.Volume = GetFloat(entryMap, "volume", 1);
						data.Balance = GetFloat(entryMap, "balance", 0);
					}
					skeletonData.Events.Add(data);
				}
			}

			// Animations.
			if (root.ContainsKey("animations"))
			{
				var jAnimations = root["animations"] as JObject;
				foreach (var entry in jAnimations)
				{
					try
					{
						ReadAnimation(entry.Value.ToObject<Dictionary<string, JToken>>(), entry.Key, skeletonData);
					}
					catch (Exception e)
					{
						throw new Exception("Error reading animation: " + entry.Key, e);
					}
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

		private Attachment ReadAttachment(JObject map, Skin skin, int slotIndex, string name, SkeletonData skeletonData)
		{
			float scale = this.Scale;
			name = GetString(map, "name", name);

			var typeName = GetString(map, "type", "region");
			var type = (AttachmentType)Enum.Parse(typeof(AttachmentType), typeName, true);

			string path = GetString(map, "path", name);

			switch (type)
			{
				case AttachmentType.Region:
					RegionAttachment region = attachmentLoader.NewRegionAttachment(skin, name, path);
					if (region == null) return null;
					region.Path = path;
					region.X = GetFloat(map, "x", 0) * scale;
					region.Y = GetFloat(map, "y", 0) * scale;
					region.ScaleX = GetFloat(map, "scaleX", 1);
					region.ScaleY = GetFloat(map, "scaleY", 1);
					region.Rotation = GetFloat(map, "rotation", 0);
					region.Width = GetFloat(map, "width", 32) * scale;
					region.Height = GetFloat(map, "height", 32) * scale;

					if (map.ContainsKey("color"))
					{
						var color = (string)map["color"];
						region.R = ToColor(color, 0);
						region.G = ToColor(color, 1);
						region.B = ToColor(color, 2);
						region.A = ToColor(color, 3);
					}

					region.UpdateOffset();
					return region;
				case AttachmentType.Boundingbox:
					BoundingBoxAttachment box = attachmentLoader.NewBoundingBoxAttachment(skin, name);
					if (box == null) return null;
					ReadVertices(map, box, GetInt(map, "vertexCount", 0) << 1);
					return box;
				case AttachmentType.Mesh:
				case AttachmentType.Linkedmesh:
					{
						MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path);
						if (mesh == null) return null;
						mesh.Path = path;

						if (map.ContainsKey("color"))
						{
							var color = (string)map["color"];
							mesh.R = ToColor(color, 0);
							mesh.G = ToColor(color, 1);
							mesh.B = ToColor(color, 2);
							mesh.A = ToColor(color, 3);
						}

						mesh.Width = GetFloat(map, "width", 0) * scale;
						mesh.Height = GetFloat(map, "height", 0) * scale;

						string parent = GetString(map, "parent", null);
						if (parent != null)
						{
							linkedMeshes.Add(new LinkedMesh(mesh, GetString(map, "skin", null), slotIndex, parent, GetBoolean(map, "deform", true)));
							return mesh;
						}

						float[] uvs = GetFloatArray(map, "uvs", 1);
						ReadVertices(map, mesh, uvs.Length);
						mesh.Triangles = GetIntArray(map, "triangles");
						mesh.RegionUVs = uvs;
						mesh.UpdateUVs();

						if (map.ContainsKey("hull")) mesh.HullLength = GetInt(map, "hull", 0) * 2;
						if (map.ContainsKey("edges")) mesh.Edges = GetIntArray(map, "edges");
						return mesh;
					}
				case AttachmentType.Path:
					{
						PathAttachment pathAttachment = attachmentLoader.NewPathAttachment(skin, name);
						if (pathAttachment == null) return null;
						pathAttachment.Closed = GetBoolean(map, "closed", false);
						pathAttachment.ConstantSpeed = GetBoolean(map, "constantSpeed", true);

						int vertexCount = GetInt(map, "vertexCount", 0);
						ReadVertices(map, pathAttachment, vertexCount << 1);

						// potential BOZO see Java impl
						pathAttachment.Lengths = GetFloatArray(map, "lengths", scale);
						return pathAttachment;
					}
				case AttachmentType.Point:
					{
						PointAttachment point = attachmentLoader.NewPointAttachment(skin, name);
						if (point == null) return null;
						point.X = GetFloat(map, "x", 0) * scale;
						point.Y = GetFloat(map, "y", 0) * scale;
						point.Rotation = GetFloat(map, "rotation", 0);

						//string color = GetString(map, "color", null);
						//if (color != null) point.color = color;
						return point;
					}
				case AttachmentType.Clipping:
					{
						ClippingAttachment clip = attachmentLoader.NewClippingAttachment(skin, name);
						if (clip == null) return null;

						string end = GetString(map, "end", null);
						if (end != null)
						{
							SlotData slot = skeletonData.FindSlot(end);
							if (slot == null) throw new Exception("Clipping end slot not found: " + end);
							clip.EndSlot = slot;
						}

						ReadVertices(map, clip, GetInt(map, "vertexCount", 0) << 1);

						//string color = GetString(map, "color", null);
						// if (color != null) clip.color = color;
						return clip;
					}
			}
			return null;
		}

		private void ReadVertices(JObject map, VertexAttachment attachment, int verticesLength)
		{
			attachment.WorldVerticesLength = verticesLength;
			float[] vertices = GetFloatArray(map, "vertices", 1);
			float scale = Scale;
			if (verticesLength == vertices.Length)
			{
				if (scale != 1)
				{
					for (int i = 0;i < vertices.Length;i++)
					{
						vertices[i] *= scale;
					}
				}
				attachment.Vertices = vertices;
				return;
			}
			ExposedList<float> weights = new ExposedList<float>(verticesLength * 3 * 3);
			ExposedList<int> bones = new ExposedList<int>(verticesLength * 3);
			for (int i = 0, n = vertices.Length;i < n;)
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

		private void ReadAnimation(Dictionary<string, JToken> map, string name, SkeletonData skeletonData)
		{
			var scale = this.Scale;
			var timelines = new ExposedList<Timeline>();
			float duration = 0;

			// Slot timelines.
			if (map.ContainsKey("slots"))
			{
				foreach (var entry in (map["slots"] as JObject))
				{
					string slotName = entry.Key;
					int slotIndex = skeletonData.FindSlotIndex(slotName);
					var timelineMap = entry.Value as JObject;
					foreach (KeyValuePair<string, JToken> timelineEntry in timelineMap)
					{
						var values = timelineEntry.Value as JArray;
						var timelineName = (string)timelineEntry.Key;
						if (timelineName == "attachment")
						{
							var timeline = new AttachmentTimeline(values.Count);
							timeline.SlotIndex = slotIndex;

							int frameIndex = 0;
							foreach (var valueMap in values)
							{
								float time = GetFloat(valueMap as JObject, "time", 0);
								timeline.SetFrame(frameIndex++, time, (string)valueMap["name"]);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);

						}
						else if (timelineName == "color")
						{
							var timeline = new ColorTimeline(values.Count);
							timeline.SlotIndex = slotIndex;

							int frameIndex = 0;
							foreach (var valueMap in values)
							{
								float time = GetFloat(valueMap as JObject, "time", 0);
								string c = (string)valueMap["color"];
								timeline.SetFrame(frameIndex, time, ToColor(c, 0), ToColor(c, 1), ToColor(c, 2), ToColor(c, 3));
								ReadCurve(valueMap as JObject, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * ColorTimeline.ENTRIES]);

						}
						else if (timelineName == "twoColor")
						{
							var timeline = new TwoColorTimeline(values.Count);
							timeline.SlotIndex = slotIndex;

							int frameIndex = 0;
							foreach (var valueMap in values)
							{
								float time = GetFloat(valueMap as JObject, "time", 0);
								string light = (string)valueMap["light"];
								string dark = (string)valueMap["dark"];
								timeline.SetFrame(frameIndex, time, ToColor(light, 0), ToColor(light, 1), ToColor(light, 2), ToColor(light, 3),
									ToColor(dark, 0, 6), ToColor(dark, 1, 6), ToColor(dark, 2, 6));
								ReadCurve(valueMap as JObject, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * TwoColorTimeline.ENTRIES]);

						}
						else
							throw new Exception("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
					}
				}
			}

			// Bone timelines.
			if (map.ContainsKey("bones"))
			{
				foreach (var entry in (map["bones"] as JObject))
				{
					string boneName = entry.Key;
					int boneIndex = skeletonData.FindBoneIndex(boneName);
					if (boneIndex == -1) throw new Exception("Bone not found: " + boneName);
					var timelineMap = entry.Value as JObject;
					foreach (var timelineEntry in timelineMap)
					{
						var values = timelineEntry.Value as JArray;
						var timelineName = (string)timelineEntry.Key;
						if (timelineName == "rotate")
						{
							var timeline = new RotateTimeline(values.Count);
							timeline.BoneIndex = boneIndex;

							int frameIndex = 0;
							foreach (var valueMap in values)
							{
								timeline.SetFrame(frameIndex, GetFloat(valueMap as JObject, "time", 0), GetFloat(valueMap as JObject, "angle", 0));
								ReadCurve(valueMap as JObject, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * RotateTimeline.ENTRIES]);

						}
						else if (timelineName == "translate" || timelineName == "scale" || timelineName == "shear")
						{
							TranslateTimeline timeline;
							float timelineScale = 1, defaultValue = 0;
							if (timelineName == "scale")
							{
								timeline = new ScaleTimeline(values.Count);
								defaultValue = 1;
							}
							else if (timelineName == "shear")
								timeline = new ShearTimeline(values.Count);
							else
							{
								timeline = new TranslateTimeline(values.Count);
								timelineScale = scale;
							}
							timeline.BoneIndex = boneIndex;

							int frameIndex = 0;
							foreach (var valueMap in values)
							{
								float time = GetFloat(valueMap as JObject, "time", 0);
								float x = GetFloat(valueMap as JObject, "x", defaultValue);
								float y = GetFloat(valueMap as JObject, "y", defaultValue);
								timeline.SetFrame(frameIndex, time, x * timelineScale, y * timelineScale);
								ReadCurve(valueMap as JObject, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * TranslateTimeline.ENTRIES]);

						}
						else
							throw new Exception("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
					}
				}
			}

			// IK constraint timelines.
			if (map.ContainsKey("ik"))
			{
				foreach (var constraintMap in (map["ik"] as JObject))
				{
					IkConstraintData constraint = skeletonData.FindIkConstraint(constraintMap.Key);
					var values = constraintMap.Value as JArray;
					var timeline = new IkConstraintTimeline(values.Count);
					timeline.IkConstraintIndex = skeletonData.IkConstraints.IndexOf(constraint);
					int frameIndex = 0;
					foreach (var valueMap in values)
					{
						timeline.SetFrame(frameIndex, GetFloat(valueMap as JObject, "time", 0), GetFloat(valueMap as JObject, "mix", 1),
							GetFloat(valueMap as JObject, "softness", 0) * scale, GetBoolean(valueMap as JObject, "bendPositive", true) ? 1 : -1,
							GetBoolean(valueMap as JObject, "compress", false), GetBoolean(valueMap as JObject, "stretch", false));
						ReadCurve(valueMap as JObject, timeline, frameIndex);
						frameIndex++;
					}
					timelines.Add(timeline);
					duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * IkConstraintTimeline.ENTRIES]);
				}
			}

			// Transform constraint timelines.
			if (map.ContainsKey("transform"))
			{
				foreach (var constraintMap in (map["transform"] as JObject))
				{
					TransformConstraintData constraint = skeletonData.FindTransformConstraint(constraintMap.Key);
					var values = constraintMap.Value as JArray;
					var timeline = new TransformConstraintTimeline(values.Count);
					timeline.TransformConstraintIndex = skeletonData.TransformConstraints.IndexOf(constraint);
					int frameIndex = 0;
					foreach (var valueMap in values)
					{
						timeline.SetFrame(frameIndex, GetFloat(valueMap as JObject, "time", 0), GetFloat(valueMap as JObject, "rotateMix", 1),
								GetFloat(valueMap as JObject, "translateMix", 1), GetFloat(valueMap as JObject, "scaleMix", 1), GetFloat(valueMap as JObject, "shearMix", 1));
						ReadCurve(valueMap as JObject, timeline, frameIndex);
						frameIndex++;
					}
					timelines.Add(timeline);
					duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * TransformConstraintTimeline.ENTRIES]);
				}
			}

			// Path constraint timelines.
			if (map.ContainsKey("path"))
			{
				foreach (var constraintMap in (map["path"] as JObject))
				{
					int index = skeletonData.FindPathConstraintIndex(constraintMap.Key);
					if (index == -1) throw new Exception("Path constraint not found: " + constraintMap.Key);
					PathConstraintData data = skeletonData.PathConstraints.Items[index];
					var timelineMap = constraintMap.Value as JObject;
					foreach (var timelineEntry in timelineMap)
					{
						var values = timelineEntry.Value as JArray;
						var timelineName = (string)timelineEntry.Key;
						if (timelineName == "position" || timelineName == "spacing")
						{
							PathConstraintPositionTimeline timeline;
							float timelineScale = 1;
							if (timelineName == "spacing")
							{
								timeline = new PathConstraintSpacingTimeline(values.Count);
								if (data.SpacingMode == SpacingMode.Length || data.SpacingMode == SpacingMode.Fixed) timelineScale = scale;
							}
							else
							{
								timeline = new PathConstraintPositionTimeline(values.Count);
								if (data.PositionMode == PositionMode.Fixed) timelineScale = scale;
							}
							timeline.PathConstraintIndex = index;
							int frameIndex = 0;
							foreach (var valueMap in values)
							{
								timeline.SetFrame(frameIndex, GetFloat(valueMap as JObject, "time", 0), GetFloat(valueMap as JObject, timelineName, 0) * timelineScale);
								ReadCurve(valueMap as JObject, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * PathConstraintPositionTimeline.ENTRIES]);
						}
						else if (timelineName == "mix")
						{
							PathConstraintMixTimeline timeline = new PathConstraintMixTimeline(values.Count);
							timeline.PathConstraintIndex = index;
							int frameIndex = 0;
							foreach (var valueMap in values)
							{
								timeline.SetFrame(frameIndex, GetFloat(valueMap as JObject, "time", 0), GetFloat(valueMap as JObject, "rotateMix", 1),
									GetFloat(valueMap as JObject, "translateMix", 1));
								ReadCurve(valueMap as JObject, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.Frames[(timeline.FrameCount - 1) * PathConstraintMixTimeline.ENTRIES]);
						}
					}
				}
			}

			// Deform timelines.
			if (map.ContainsKey("deform"))
			{
				foreach (var deformMap in (map["deform"] as JObject))
				{
					Skin skin = skeletonData.FindSkin(deformMap.Key);
					foreach (var slotMap in (deformMap.Value as JObject))
					{
						int slotIndex = skeletonData.FindSlotIndex(slotMap.Key);
						if (slotIndex == -1) throw new Exception("Slot not found: " + slotMap.Key);
						foreach (var timelineMap in (slotMap.Value as JObject))
						{
							var values = timelineMap.Value as JArray;
							VertexAttachment attachment = (VertexAttachment)skin.GetAttachment(slotIndex, timelineMap.Key);
							if (attachment == null) throw new Exception("Deform attachment not found: " + timelineMap.Key);
							bool weighted = attachment.Bones != null;
							float[] vertices = attachment.Vertices;
							int deformLength = weighted ? vertices.Length / 3 * 2 : vertices.Length;

							var timeline = new DeformTimeline(values.Count);
							timeline.SlotIndex = slotIndex;
							timeline.Attachment = attachment;

							int frameIndex = 0;
							foreach (var valueMap in values)
							{
								float[] deform;
								if (!(valueMap as JObject).ContainsKey("vertices"))
								{
									deform = weighted ? new float[deformLength] : vertices;
								}
								else
								{
									deform = new float[deformLength];
									int start = GetInt(valueMap as JObject, "offset", 0);
									float[] verticesValue = GetFloatArray(valueMap as JObject, "vertices", 1);
									Array.Copy(verticesValue, 0, deform, start, verticesValue.Length);
									if (scale != 1)
									{
										for (int i = start, n = i + verticesValue.Length;i < n;i++)
											deform[i] *= scale;
									}

									if (!weighted)
									{
										for (int i = 0;i < deformLength;i++)
											deform[i] += vertices[i];
									}
								}

								timeline.SetFrame(frameIndex, GetFloat(valueMap as JObject, "time", 0), deform);
								ReadCurve(valueMap as JObject, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
						}
					}
				}
			}

			// Draw order timeline.
			if (map.ContainsKey("drawOrder") || map.ContainsKey("draworder"))
			{
				var values = map[map.ContainsKey("drawOrder") ? "drawOrder" : "draworder"] as JArray;
				var timeline = new DrawOrderTimeline(values.Count);
				int slotCount = skeletonData.Slots.Count;
				int frameIndex = 0;
				foreach (var drawOrderMap in values)
				{
					int[] drawOrder = null;
					if ((drawOrderMap as JObject).ContainsKey("offsets"))
					{
						drawOrder = new int[slotCount];
						for (int i = slotCount - 1;i >= 0;i--)
							drawOrder[i] = -1;
						var offsets = drawOrderMap["offsets"] as JArray;
						int[] unchanged = new int[slotCount - offsets.Count];
						int originalIndex = 0, unchangedIndex = 0;
						foreach (var offsetMap in offsets)
						{
							int slotIndex = skeletonData.FindSlotIndex((string)offsetMap["slot"]);
							if (slotIndex == -1) throw new Exception("Slot not found: " + offsetMap["slot"]);
							// Collect unchanged items.
							while (originalIndex != slotIndex)
								unchanged[unchangedIndex++] = originalIndex++;
							// Set changed items.
							int index = originalIndex + (int)(float)offsetMap["offset"];
							drawOrder[index] = originalIndex++;
						}
						// Collect remaining unchanged items.
						while (originalIndex < slotCount)
							unchanged[unchangedIndex++] = originalIndex++;
						// Fill in unchanged items.
						for (int i = slotCount - 1;i >= 0;i--)
							if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
					}
					timeline.SetFrame(frameIndex++, GetFloat(drawOrderMap as JObject, "time", 0), drawOrder);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
			}

			// Event timeline.
			if (map.ContainsKey("events"))
			{
				var eventsMap = map["events"] as JArray;
				var timeline = new EventTimeline(eventsMap.Count);
				int frameIndex = 0;
				foreach (var eventMap in eventsMap)
				{
					EventData eventData = skeletonData.FindEvent((string)eventMap["name"]);
					if (eventData == null) throw new Exception("Event not found: " + eventMap["name"]);
					var e = new Event(GetFloat(eventMap as JObject, "time", 0), eventData);
					e.Int = GetInt(eventMap as JObject, "int", eventData.Int);
					e.Float = GetFloat(eventMap as JObject, "float", eventData.Float);
					e.String = GetString(eventMap as JObject, "string", eventData.String);
					if (e.Data.AudioPath != null)
					{
						e.Volume = GetFloat(eventMap as JObject, "volume", eventData.Volume);
						e.Balance = GetFloat(eventMap as JObject, "balance", eventData.Balance);
					}
					timeline.SetFrame(frameIndex++, e);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
			}

			timelines.TrimExcess();
			skeletonData.Animations.Add(new Animation(name, timelines, duration));
		}

		static void ReadCurve(JObject valueMap, CurveTimeline timeline, int frameIndex)
		{
			if (!valueMap.ContainsKey("curve"))
				return;
			var curveObject = valueMap["curve"];
			if (curveObject.Type == JTokenType.String)
			{
				if (float.TryParse((string)curveObject, out var val))
				{
					timeline.SetCurve(frameIndex, val, GetFloat(valueMap, "c2", 0), GetFloat(valueMap, "c3", 1), GetFloat(valueMap, "c4", 1));
				}
				else
				{
					timeline.SetStepped(frameIndex);
				}
			}
			else
				timeline.SetCurve(frameIndex, (float)curveObject, GetFloat(valueMap, "c2", 0), GetFloat(valueMap, "c3", 1), GetFloat(valueMap, "c4", 1));
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

		static float[] GetFloatArray(JObject map, string name, float scale)
		{
			var list = map[name] as JArray;
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

		static int[] GetIntArray(JObject map, string name)
		{
			var list = map[name] as JArray;
			var values = new int[list.Count];
			for (int i = 0, n = list.Count;i < n;i++)
				values[i] = (int)(float)list[i];
			return values;
		}

		static float GetFloat(JObject map, string name, float defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (float)map[name];
		}

		static int GetInt(JObject map, string name, int defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (int)(float)map[name];
		}

		static bool GetBoolean(JObject map, string name, bool defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (bool)map[name];
		}

		static string GetString(JObject map, string name, string defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (string)map[name];
		}

		static float ToColor(string hexString, int colorIndex, int expectedLength = 8)
		{
			if (hexString.Length != expectedLength)
				throw new ArgumentException("Color hexidecimal length must be " + expectedLength + ", recieved: " + hexString, "hexString");
			return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
		}
	}
}
