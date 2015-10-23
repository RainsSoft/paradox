using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.SpriteStudio.Runtime;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    internal class SpriteStudioModelAssetCompiler : AssetCompilerBase<SpriteStudioModelAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SpriteStudioModelAsset asset, AssetCompilerResult result)
        {
            var gameSettingsAsset = context.GetGameSettingsAsset();
            var colorSpace = context.GetColorSpace();

            var cells = new List<SpriteStudioCell>();
            var images = new List<UFile>();
            if (!SpriteStudioXmlImport.ParseCellMaps(asset.Source, images, cells)) throw new Exception("Failed to parse Sprite Studio cell textures.");

            var texIndex = 0;
            asset.BuildTextures.Clear();
            foreach (var texture in images)
            {
                var textureAsset = new TextureAsset
                {
                    Id = Guid.Empty, // CAUTION: It is important to use an empty GUID here, as we don't want the command to be rebuilt (by default, a new asset is creating a new guid)
                    Alpha = AlphaFormat.Auto,
                    Format = TextureFormat.Color32Bits,
                    GenerateMipmaps = true,
                    PremultiplyAlpha = true,
                    ColorSpace = TextureColorSpace.Auto,
                    Hint = TextureHint.Color
                };

                result.BuildSteps.Add(
                new TextureAssetCompiler.TextureConvertCommand(
                    urlInStorage + texIndex,
                    new TextureConvertParameters(texture, textureAsset, context.Platform,
                        context.GetGraphicsPlatform(), gameSettingsAsset.DefaultGraphicsProfile,
                        gameSettingsAsset.TextureQuality, colorSpace)));

                asset.BuildTextures.Add(urlInStorage + texIndex);

                texIndex++;
            }

            result.BuildSteps.Add(new AssetBuildStep(AssetItem)
            {
                new SpriteStudioModelAssetCommand(urlInStorage, asset, colorSpace)
            });
        }

        /// <summary>
        /// Command used by the build engine to convert the asset
        /// </summary>
        private class SpriteStudioModelAssetCommand : AssetCommand<SpriteStudioModelAsset>
        {
            private readonly ColorSpace colorSpace;

            public SpriteStudioModelAssetCommand(string url, SpriteStudioModelAsset asset, ColorSpace colorSpace)
                : base(url, asset)
            {
                this.colorSpace = colorSpace;
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var nodes = new List<SpriteStudioNode>();
                string modelName;
                if (!SpriteStudioXmlImport.ParseModel(AssetParameters.Source, nodes, out modelName)) return null;

                var cells = new List<SpriteStudioCell>();
                var textures = new List<UFile>();
                if (!SpriteStudioXmlImport.ParseCellMaps(AssetParameters.Source, textures, cells)) return null;

                var anims = new List<SpriteStudioAnim>();
                if (!SpriteStudioXmlImport.ParseAnimations(AssetParameters.Source, anims)) return null;             

                var assetManager = new AssetManager();

                var sheet = new SpriteSheet();

                foreach (var cell in cells)
                {
                    var sprite = new Sprite(cell.Name, AttachedReferenceManager.CreateSerializableVersion<Texture>(Guid.Empty, AssetParameters.BuildTextures[cell.TextureIndex]))
                    {
                        Region = cell.Rectangle,
                        Center = cell.Pivot,
                        IsTransparent = true
                    };
                    sheet.Sprites.Add(sprite);
                }

                //fill up some basic data for our model using the first animation in the array
                var anim = anims[0];
                foreach (var data in anim.NodesData)
                {
                    var node = nodes.FirstOrDefault(x => x.Name == data.Key);
                    if (node == null) continue;

                    foreach (var pair in data.Value.Data)
                    {
                        var tag = pair.Key;
                        if(pair.Value.All(x => x["time"] != "0")) continue;
                        var value = pair.Value.First()["value"]; //do we always have a frame 0? should be the case actually
                        switch (tag)
                        {
                            case "POSX":
                                node.BaseState.Position.X = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "POSY":
                                node.BaseState.Position.Y = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "ROTZ":
                                node.BaseState.RotationZ = MathUtil.DegreesToRadians(float.Parse(value, CultureInfo.InvariantCulture));
                                break;
                            case "PRIO":
                                node.BaseState.Priority = int.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "SCLX":
                                node.BaseState.Scale.X = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "SCLY":
                                node.BaseState.Scale.Y = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "ALPH":
                                node.BaseState.Transparency = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "HIDE":
                                node.BaseState.Hide = int.Parse(value, CultureInfo.InvariantCulture) > 0;
                                break;
                            case "FLPH":
                                node.BaseState.HFlipped = int.Parse(value, CultureInfo.InvariantCulture) > 0;
                                break;
                            case "FLPV":
                                node.BaseState.VFlipped = int.Parse(value, CultureInfo.InvariantCulture) > 0;
                                break;
                            case "CELL":
                                node.BaseState.SpriteId = int.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "COLV":
                                var color = new Color4(Color.FromBgra(int.Parse(value, CultureInfo.InvariantCulture)));
                                node.BaseState.BlendColor = colorSpace == ColorSpace.Linear ? color.ToLinear() : color;
                                break;
                            case "COLB":
                                node.BaseState.BlendType = (SpriteStudioBlending)int.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "COLF":
                                node.BaseState.BlendFactor = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                        }
                    }
                }

                var spriteStudioSheet = new SpriteStudioSheet
                {
                    NodesInfo = nodes,
                    SpriteSheet = sheet
                };

                assetManager.Save(Url, spriteStudioSheet);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}