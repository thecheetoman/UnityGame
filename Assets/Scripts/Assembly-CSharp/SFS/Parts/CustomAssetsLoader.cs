using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
using ModLoader;
using ModLoader.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SFS.Builds;
using SFS.IO;
using SFS.Parts.Modules;
using SFS.UI;
using UnityEngine;

namespace SFS.Parts
{
	public static class CustomAssetsLoader
	{
		private class T2DConverter : JsonConverter<Texture2D>
		{
			public override Texture2D ReadJson(JsonReader reader, Type objectType, Texture2D existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				if (reader.Value == null || string.IsNullOrWhiteSpace((string)reader.Value))
				{
					return null;
				}
				Texture2D texture2D = TextureUtility.FromFile(new FilePath((string)reader.Value));
				texture2D.wrapMode = TextureWrapMode.Clamp;
				return texture2D;
			}

			public override void WriteJson(JsonWriter writer, Texture2D value, JsonSerializer serializer)
			{
				string text = value.name + ".png";
				writer.WriteValue(text);
				FilePath filePath = currentPath.ExtendToFile(text);
				value.SaveToFile(filePath);
			}
		}

		private class ShadowTextureConverter : JsonConverter<ShadowTexture>
		{
			public override void WriteJson(JsonWriter writer, ShadowTexture value, JsonSerializer serializer)
			{
				if (shapeTextures)
				{
					writer.WriteValue(value.name);
				}
				else
				{
					serializerForShadowTexture.Serialize(writer, value, typeof(ShadowTexture));
				}
			}

			public override ShadowTexture ReadJson(JsonReader reader, Type objectType, ShadowTexture existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				if (!shapeTextures)
				{
					return serializer.Deserialize<ShadowTexture>(reader);
				}
				return shadowTextures.GetValueOrDefault((string)reader.Value);
			}
		}

		private class SpriteConverter : JsonConverter<Sprite>
		{
			public override void WriteJson(JsonWriter writer, Sprite value, JsonSerializer serializer)
			{
				writer.WriteValue((object?)null);
			}

			public override Sprite ReadJson(JsonReader reader, Type objectType, Sprite existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				return null;
			}
		}

		private static readonly StringBuilder Report = new StringBuilder();

		public static bool finishedLoading;

		private static bool loadedAssetPacks;

		private static bool loadedTexturePacks;

		private static JsonSerializer serializer;

		private static JsonSerializer serializerForShadowTexture;

		private static FolderPath currentPath;

		private static bool shapeTextures;

		private static Dictionary<string, ShadowTexture> shadowTextures;

		public static FolderPath CustomAssetsFolder => Loader.ModsFolder.Extend("Custom Assets").CreateFolder();

		public static async UniTask LoadAllCustomAssets()
		{
			await LoadAssetPacks();
			await LoadTexturePacks();
			if (Report.Length > 0)
			{
				Menu.read.ShowReport(Report, delegate
				{
				});
			}
			finishedLoading = true;
		}

		private static async UniTask LoadAssetPacks()
		{
			if (Application.isEditor || !DevSettings.FullVersion || loadedAssetPacks)
			{
				return;
			}
			loadedAssetPacks = true;
			Dictionary<string, ResourceType> resourceTypes = ResourcesLoader.GetFiles_Dictionary<ResourceType>("");
			Dictionary<string, PickCategory> pickCategories = ResourcesLoader.GetFiles_Dictionary<PickCategory>("");
			await UniTask.Yield();
			foreach (FilePath path in CustomAssetsFolder.Extend("Parts").CreateFolder().GetFilesInFolder(recursively: false))
			{
				try
				{
					AssetBundlePack bundlePack = await UniTask.RunOnThreadPool(delegate
					{
						try
						{
							return JsonConvert.DeserializeObject<AssetBundlePack>(path.ReadText());
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
						return (AssetBundlePack)null;
					});
					if (bundlePack == null)
					{
						throw new Exception("Can't deserialize pack file");
					}
					if (bundlePack.Data == null)
					{
						throw new Exception("Pack doesn't support current runtime platform");
					}
					if (bundlePack.CodeAssembly != null)
					{
						await UniTask.RunOnThreadPool(delegate
						{
							try
							{
								Assembly.Load(bundlePack.CodeAssembly);
							}
							catch (Exception exception)
							{
								Debug.Log("Failed to load custom scripts for " + path.CleanFileName + " pack");
								Debug.LogException(exception);
							}
						});
					}
					UnityEngine.Object[] source = (await AssetBundle.LoadFromMemoryAsync(bundlePack.Data)).LoadAllAssets();
					if (!ModsSettings.main.settings.assetPacksActive.ContainsKey(path.FileName))
					{
						ModsSettings.main.settings.assetPacksActive.Add(path.FileName, value: true);
					}
					ModsListElement.ModData data;
					if (source.Any((UnityEngine.Object x) => x.GetType() == typeof(PackData)))
					{
						PackData packData = source.OfType<PackData>().First();
						data = new ModsListElement.ModData
						{
							name = packData.DisplayName,
							author = packData.Author,
							description = packData.Description,
							icon = (packData.ShowIcon ? packData.Icon : null),
							type = ModsListElement.ModType.AssetsPack,
							version = packData.Version,
							saveName = path.FileName
						};
					}
					else
					{
						data = new ModsListElement.ModData
						{
							name = path.FileName,
							author = "Unknown Author",
							description = "No description",
							icon = null,
							type = ModsListElement.ModType.AssetsPack,
							version = "1.0",
							saveName = path.FileName
						};
					}
					ModsMenu.AddElement(data);
					if (!ModsSettings.main.settings.assetPacksActive[path.FileName])
					{
						continue;
					}
					foreach (ResourceType item in source.OfType<ResourceType>())
					{
						if (!resourceTypes.ContainsKey(item.name))
						{
							resourceTypes.Add(item.name, item);
						}
					}
					foreach (PickCategory item2 in source.OfType<PickCategory>())
					{
						if (!pickCategories.ContainsKey(item2.name))
						{
							pickCategories.Add(item2.name, item2);
						}
					}
					foreach (ColorTexture item3 in source.OfType<ColorTexture>())
					{
						if (!Base.partsLoader.colorTextures.ContainsKey(item3.name))
						{
							Base.partsLoader.colorTextures.Add(item3.name, item3);
							continue;
						}
						ColorTexture colorTexture = Base.partsLoader.colorTextures[item3.name];
						item3.multiple = colorTexture.multiple;
						item3.colorTex = colorTexture.colorTex;
						item3.segments = colorTexture.segments;
					}
					foreach (ShapeTexture item4 in source.OfType<ShapeTexture>())
					{
						if (!Base.partsLoader.shapeTextures.ContainsKey(item4.name))
						{
							Base.partsLoader.shapeTextures.Add(item4.name, item4);
							continue;
						}
						ShapeTexture shapeTexture = Base.partsLoader.shapeTextures[item4.name];
						item4.multiple = shapeTexture.multiple;
						item4.shapeTex = shapeTexture.shapeTex;
						item4.segments = shapeTexture.segments;
						item4.shadowTex = shapeTexture.shadowTex;
					}
					foreach (GameObject item5 in source.OfType<GameObject>())
					{
						if (!item5.HasComponent<Part>(out var component))
						{
							continue;
						}
						Variants[] variants = component.variants;
						foreach (Variants variants2 in variants)
						{
							Variants.Variant[] variants3 = variants2.variants;
							for (int num2 = 0; num2 < variants3.Length; num2++)
							{
								Variants.PickTag[] tags = variants3[num2].tags;
								foreach (Variants.PickTag pickTag in tags)
								{
									pickTag.tag = pickCategories.GetValueOrDefault(pickTag.tag.name, pickTag.tag);
								}
							}
							variants2.tags = Array.Empty<Variants.PickTag>();
						}
						FlowModule[] componentsInChildren = item5.GetComponentsInChildren<FlowModule>();
						for (int num = 0; num < componentsInChildren.Length; num++)
						{
							FlowModule.Flow[] sources = componentsInChildren[num].sources;
							foreach (FlowModule.Flow flow in sources)
							{
								flow.resourceType = resourceTypes.GetValueOrDefault(flow.resourceType.name, flow.resourceType);
							}
						}
						ResourceModule[] componentsInChildren2 = item5.GetComponentsInChildren<ResourceModule>();
						foreach (ResourceModule resourceModule in componentsInChildren2)
						{
							resourceModule.resourceType = resourceTypes.GetValueOrDefault(resourceModule.resourceType.name, resourceModule.resourceType);
						}
						if (!Base.partsLoader.parts.ContainsKey(component.name))
						{
							Base.partsLoader.parts.Add(component.name, component);
						}
						for (int num4 = 0; num4 < component.variants.Length; num4++)
						{
							for (int num5 = 0; num5 < component.variants[num4].variants.Length; num5++)
							{
								VariantRef variantRef = new VariantRef(component, num4, num5);
								if (!Base.partsLoader.partVariants.ContainsKey(variantRef.GetNameID()))
								{
									Base.partsLoader.partVariants.Add(variantRef.GetNameID(), variantRef);
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Report.AppendLine("Failed to load asset pack: " + path);
					Report.AppendLine(ex.Message);
				}
			}
			GC.Collect();
		}

		private static async UniTask LoadTexturePacks()
		{
			if (Application.isEditor || !DevSettings.FullVersion || loadedTexturePacks)
			{
				return;
			}
			loadedTexturePacks = true;
			FolderPath folderPath = CustomAssetsFolder.Extend("Texture Packs").CreateFolder();
			serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
			{
				MaxDepth = 10,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				Converters = new List<JsonConverter>
				{
					new StringEnumConverter
					{
						AllowIntegerValues = true
					},
					new T2DConverter(),
					new ShadowTextureConverter(),
					new SpriteConverter()
				},
				Formatting = Formatting.Indented
			});
			serializerForShadowTexture = JsonSerializer.CreateDefault(new JsonSerializerSettings
			{
				MaxDepth = 10,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				Converters = new List<JsonConverter>
				{
					new StringEnumConverter
					{
						AllowIntegerValues = true
					},
					new T2DConverter(),
					new SpriteConverter()
				},
				Formatting = Formatting.Indented
			});
			shadowTextures = ResourcesLoader.GetFiles_Dictionary<ShadowTexture>("");
			CreateExampleTexturePack(folderPath);
			foreach (FolderPath item in folderPath.GetFoldersInFolder(recursively: false))
			{
				if (!(item.FolderName == "Example"))
				{
					await LoadTexturePack(item);
				}
			}
		}

		private static void CreateExampleTexturePack(FolderPath basePath)
		{
			FolderPath folderPath = basePath.CloneAndExtend("Example");
			if (folderPath.FolderExists())
			{
				return;
			}
			folderPath.CreateFolder();
			foreach (KeyValuePair<string, ColorTexture> colorTexture in Base.partsLoader.colorTextures)
			{
				currentPath = folderPath.CloneAndExtend("Color Textures").Extend(colorTexture.Key).CreateFolder();
				serializer.SerializeTo(currentPath.ExtendToFile("config.txt"), colorTexture.Value, typeof(ColorTexture));
			}
			foreach (KeyValuePair<string, ShadowTexture> shadowTexture in shadowTextures)
			{
				currentPath = folderPath.CloneAndExtend("Shadow Textures").Extend(shadowTexture.Key).CreateFolder();
				serializer.SerializeTo(currentPath.ExtendToFile("config.txt"), shadowTexture.Value, typeof(ShadowTexture));
			}
			shapeTextures = true;
			foreach (KeyValuePair<string, ShapeTexture> shapeTexture in Base.partsLoader.shapeTextures)
			{
				currentPath = folderPath.CloneAndExtend("Shape Textures").Extend(shapeTexture.Key).CreateFolder();
				serializer.SerializeTo(currentPath.ExtendToFile("config.txt"), shapeTexture.Value, typeof(ShapeTexture));
			}
			shapeTextures = false;
			PackData packData = ScriptableObject.CreateInstance<PackData>();
			packData.Author = "Spaceflight Simulator";
			packData.Description = "Example pack with game textures";
			packData.Icon = null;
			packData.Version = "1.0";
			packData.DisplayName = "Example Textures";
			packData.ShowIcon = false;
			currentPath = folderPath;
			serializer.SerializeTo(folderPath.ExtendToFile("pack_info.txt"), packData, typeof(PackData));
		}

		private static async UniTask LoadTexturePack(FolderPath packPath)
		{
			if (!ModsSettings.main.settings.texturePacksActive.ContainsKey(packPath.FolderName))
			{
				ModsSettings.main.settings.texturePacksActive.Add(packPath.FolderName, value: true);
			}
			FilePath filePath = packPath.ExtendToFile("pack_info.txt");
			ModsListElement.ModData data;
			if (filePath.FileExists())
			{
				try
				{
					PackData packData = ScriptableObject.CreateInstance<PackData>();
					JObject jObject = JObject.Parse(filePath.ReadText());
					jObject["Icon"]?.Replace((string)packPath.ExtendToFile((string?)jObject["Icon"]));
					jObject.Populate(packData, serializer);
					data = new ModsListElement.ModData
					{
						author = packData.Author,
						description = packData.Description,
						icon = packData.Icon,
						name = packData.DisplayName,
						type = ModsListElement.ModType.TexturesPack,
						version = packData.Version,
						saveName = packPath.FolderName
					};
				}
				catch
				{
					Report.AppendLine("Failed to load texture pack info file: " + packPath.FolderName);
					data = new ModsListElement.ModData
					{
						name = packPath.FolderName,
						author = "Unknown Author",
						description = "No description",
						icon = null,
						type = ModsListElement.ModType.TexturesPack,
						version = "1.0",
						saveName = packPath.FolderName
					};
				}
			}
			else
			{
				data = new ModsListElement.ModData
				{
					name = packPath.FolderName,
					author = "Unknown Author",
					description = "No description",
					icon = null,
					type = ModsListElement.ModType.TexturesPack,
					version = "1.0",
					saveName = packPath.FolderName
				};
			}
			ModsMenu.AddElement(data);
			if (!ModsSettings.main.settings.texturePacksActive[packPath.FolderName])
			{
				return;
			}
			if (packPath.CloneAndExtend("Color Textures").FolderExists())
			{
				foreach (FolderPath item in packPath.CloneAndExtend("Color Textures").GetFoldersInFolder(recursively: false))
				{
					try
					{
						FilePath filePath2 = item.ExtendToFile("config.txt");
						if (!filePath2.FileExists())
						{
							throw new FileNotFoundException("Config file not found!");
						}
						ColorTexture colorTexture = ScriptableObject.CreateInstance<ColorTexture>();
						JObject jObject2 = JObject.Parse(filePath2.ReadText());
						foreach (JToken item2 in (IEnumerable<JToken>)(jObject2["colorTex"]?["textures"]))
						{
							item2["texture"]?.Replace((string)item.ExtendToFile((string?)item2["texture"]));
						}
						jObject2.Populate(colorTexture, serializer);
						Base.partsLoader.colorTextures.Add(colorTexture.name, colorTexture);
					}
					catch (Exception ex)
					{
						Report.AppendLine("Failed to load " + item.FolderName + " texture in " + packPath.FolderName + " pack:");
						Report.AppendLine(ex.Message);
					}
				}
				await UniTask.Yield();
			}
			if (packPath.CloneAndExtend("Shadow Textures").FolderExists())
			{
				foreach (FolderPath item3 in packPath.CloneAndExtend("Shadow Textures").GetFoldersInFolder(recursively: false))
				{
					try
					{
						FilePath filePath3 = item3.ExtendToFile("config.txt");
						if (!filePath3.FileExists())
						{
							throw new FileNotFoundException("Config file not found!");
						}
						ShadowTexture shadowTexture = ScriptableObject.CreateInstance<ShadowTexture>();
						JObject jObject3 = JObject.Parse(filePath3.ReadText());
						foreach (JToken item4 in (IEnumerable<JToken>)jObject3["texture"]["textures"])
						{
							item4["texture"].Replace((string)item3.ExtendToFile((string?)item4["texture"]));
						}
						jObject3.Populate(shadowTexture, serializer);
						shadowTextures.Add(shadowTexture.name, shadowTexture);
					}
					catch (Exception ex2)
					{
						Report.AppendLine("Failed to load " + item3.FolderName + " texture in " + packPath.FolderName + " pack:");
						Report.AppendLine(ex2.Message);
					}
				}
				await UniTask.Yield();
			}
			if (!packPath.CloneAndExtend("Shape Textures").FolderExists())
			{
				return;
			}
			shapeTextures = true;
			foreach (FolderPath item5 in packPath.CloneAndExtend("Shape Textures").GetFoldersInFolder(recursively: false))
			{
				try
				{
					currentPath = item5;
					FilePath filePath4 = currentPath.ExtendToFile("config.txt");
					if (!filePath4.FileExists())
					{
						throw new FileNotFoundException("Config file not found!");
					}
					ShapeTexture shapeTexture = ScriptableObject.CreateInstance<ShapeTexture>();
					JObject jObject4 = JObject.Parse(filePath4.ReadText());
					foreach (JToken item6 in (IEnumerable<JToken>)jObject4["shapeTex"]["textures"])
					{
						item6["texture"].Replace((string)item5.ExtendToFile((string?)item6["texture"]));
					}
					jObject4.Populate(shapeTexture, serializer);
					Base.partsLoader.shapeTextures.Add(shapeTexture.name, shapeTexture);
				}
				catch (Exception ex3)
				{
					Report.AppendLine("Failed to load " + item5.FolderName + " texture in " + packPath.FolderName + " pack:");
					Report.AppendLine(ex3.Message);
				}
			}
			shapeTextures = false;
			await UniTask.Yield();
		}
	}
}
