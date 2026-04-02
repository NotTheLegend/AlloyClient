using System;
using System.Linq;
using AlloyClient.Assets.Libraries;
using AlloyClient.Rendering.VertexData;
using AlloyClient.Utils;
using Common.Structs;
using OpenTK.Mathematics;

namespace AlloyClient.Game;

public static class TileBuilder {

    private static readonly Vector4[] Masks = Main.Atlas.GetAtlasData("tileAlphaBlend").Select(x => x.ToVector4(true)).ToArray();

    public static int Build(MapTile self, Span<TileData> data, Span<MapTile> tiles) {
        if (self.Type == Const.CompositeTile) {
            return BuildComposite(data, tiles);
        }

        if (self.GroundProperties.HasEdge) {
            return BuildEdges(data, tiles);
        }

        return BuildBlends(data, tiles);
    }

    private static int BuildComposite(Span<TileData> data, Span<MapTile> tiles) {
        var count = 0;
        var self = tiles[4];
        
        MapTile tile;
        MapTile tile1;
        MapTile tile2;
        MapTile tile3;
        MapTile tile4;

        var n1 = tiles[1];
        var n3 = tiles[3];
        var n5 = tiles[5];
        var n7 = tiles[7];
        
        var p1 = n1 != null ? n1.GroundProperties.CompositePriority : -1;
        var p3 = n3 != null ? n3.GroundProperties.CompositePriority : -1;
        var p5 = n5 != null ? n5.GroundProperties.CompositePriority : -1;
        var p7 = n7 != null ? n7.GroundProperties.CompositePriority : -1;
        
        
        if (p1 < 0 && p3 < 0) {
            tile = tiles[0];
            tile1 = (tile == null || tile.GroundProperties.CompositePriority < 0) ? null : tile;
        } else if (p1 < p3) {
            tile1 = n3;
        } else {
            tile1 = n1;
        }

        if (p1 < 0 && p5 < 0) {
            tile = tiles[2];
            tile2 = (tile == null || tile?.GroundProperties.CompositePriority < 0) ? null : tile;
        } else if (p1 < p5) {
            tile2 = n5;
        } else {
            tile2 = n1;
        }

        if (p3 < 0 && p7 < 0) {
            tile = tiles[6];
            tile3 = (tile == null || tile.GroundProperties.CompositePriority < 0) ? null : tile;
        } else if (p3 < p7) {
            tile3 = n7;
        } else {
            tile3 = n3;
        }

        if (p5 < 0 && p7 < 0) {
            tile = tiles[8];
            tile4 = (tile == null || tile.GroundProperties.CompositePriority < 0) ? null : tile;
        } else if (p5 < p7) {
            tile4 = n7;
        } else {
            tile4 = n5;
        }
        
        if(tile1 != null && tile1.Type != Const.DefaultTile) {
            data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(100));
        }

        if(tile2 != null && tile2.Type != Const.DefaultTile) {
            data[count++] = tile2.CloneWithBlend(self.X, self.Y, GetMask(101));
        }

        if(tile3 != null && tile3.Type != Const.DefaultTile) {
            data[count++] = tile3.CloneWithBlend(self.X, self.Y, GetMask(102));
        }

        if(tile4 != null && tile4.Type != Const.DefaultTile) {
            data[count++] = tile4.CloneWithBlend(self.X, self.Y, GetMask(103));
        }

        return count;
    }

    private static int BuildEdges(Span<TileData> data, Span<MapTile> tiles) {
        var count = 0;
        var self = tiles[4];

        Span<bool> sig = stackalloc bool[9];

        var b = false;
        var hasEdge = false;
        var sameTypeEdgeMode = self.GroundProperties.SameTypeEdgeMode;

        var idx = 0;
        
        for (var y = -1; y <= 1; y++) {
            for (var x = -1; x <= 1; x++) {

                var temp = tiles[idx];

                if (temp != null && temp.X == self.X && temp.Y == self.Y) {
                    sig[idx] = true;
                } else {
                    if (sameTypeEdgeMode) {
                        b = temp == null || temp.Type == self.Type;
                    } else {
                        b = temp == null || temp.Type != Const.DefaultTile;
                    }
                    sig[idx] = b;
                    hasEdge = hasEdge || !b;
                }

                idx++;
            }
        }

        var texture = GroundLibrary.TypeToTextureData[self.Type];
        var edgeTexture = texture.EdgeTexture;

        if (!hasEdge || edgeTexture == null) {
            return count;
        }

        var edge = edgeTexture.GetTexture();
        edge.RemovePadding();
        
        if (!sig[1]) { // top
            data[count++] = self.CloneWithEdge(edge.Rotate(1), true);
        }
        
        if (!sig[3]) { // left
            data[count++] = self.CloneWithEdge(edge.Rotate(0));
        }
        
        if (!sig[5]) { // right
            data[count++] = self.CloneWithEdge(edge.Rotate(2));
        }
        
        if (!sig[7]) { // bottom
            data[count++] = self.CloneWithEdge(edge.Rotate(3), true);
        }

        var cornerTexture = texture.CornerTexture;
        if (cornerTexture != null) {
            var corner = cornerTexture.GetTexture();
            corner.RemovePadding();
            
            if (!sig[3] && !sig[1] && !sig[0]) {
                data[count++] = self.CloneWithEdge(corner.Rotate(0));
            }

            if (!sig[1] && !sig[5] && !sig[2]) {
                data[count++] = self.CloneWithEdge(corner.Rotate(1), true);
            }

            if (!sig[5] && !sig[7] && !sig[8]) {
                data[count++] = self.CloneWithEdge(corner.Rotate(2));
            }

            if (!sig[3] && !sig[7] && !sig[6]) {
                data[count++] = self.CloneWithEdge(corner.Rotate(3), true);
            }
        }

        var innerTexture = texture.InnerCornerTexture;
        if (innerTexture != null) {
            var inner = innerTexture.GetTexture();
            inner.RemovePadding();
            
            if (!sig[3] && !sig[1]) {
                data[count++] = self.CloneWithEdge(inner.Rotate(0));
            }

            if (!sig[1] && !sig[5]) {
                data[count++] = self.CloneWithEdge(inner.Rotate(1), true);
            }

            if (!sig[5] && !sig[7]) {
                data[count++] = self.CloneWithEdge(inner.Rotate(2));
            }

            if (!sig[3] && !sig[7]) {
                data[count++] = self.CloneWithEdge(inner.Rotate(3), true);
            }
        }
        
        return count;
    }

    private static int BuildBlends(Span<TileData> data, Span<MapTile> tiles) {
        var count = 0;
        var self = tiles[4];
        MapTile tile1;
        MapTile tile2;
        MapTile tile3;
        
        // top left
        tile1 = GetTile(tiles[3], self);
        tile2 = GetTile(tiles[0], self);
        tile3 = GetTile(tiles[1], self);
        
        if (self != tile1 || self != tile2 || self != tile3) {
            if (self == tile1 && self == tile3) {
                data[count++] = tile2.CloneWithBlend(self.X, self.Y, GetMask(80));
            } else if (self != tile1 && self != tile3) {
                if (tile1.Type != tile3.Type) {
                    data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(90));
                    data[count++] = tile3.CloneWithBlend(self.X, self.Y, GetMask(94));
                } else {
                    data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(84));
                }
            } else if (self != tile1) {
                data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(0));
            } else {
                data[count++] = tile3.CloneWithBlend(self.X, self.Y, GetMask(4));
            }
        }
        
        // top right
        tile1 = GetTile(tiles[1], self);
        tile2 = GetTile(tiles[2], self);
        tile3 = GetTile(tiles[5], self);
        
        if (self != tile1 || self != tile2 || self != tile3) {
            if (self == tile1 && self == tile3) {
                data[count++] = tile2.CloneWithBlend(self.X, self.Y, GetMask(81));
            } else if (self != tile1 && self != tile3) {
                if (tile1.Type != tile3.Type) {
                    data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(91));
                    data[count++] = tile3.CloneWithBlend(self.X, self.Y, GetMask(95));
                } else {
                    data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(85));
                }
            } else if (self != tile1) {
                data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(5));
            } else {
                data[count++] = tile3.CloneWithBlend(self.X, self.Y, GetMask(2));
            }
        }
        
        // bottom left
        tile1 = GetTile(tiles[7], self);
        tile2 = GetTile(tiles[6], self);
        tile3 = GetTile(tiles[3], self);
        
        if (self != tile1 || self != tile2 || self != tile3) {
            if (self == tile1 && self == tile3) {
                data[count++] = tile2.CloneWithBlend(self.X, self.Y, GetMask(82));
            } else if (self != tile1 && self != tile3) {
                if (tile1.Type != tile3.Type) {
                    data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(92));
                    data[count++] = tile3.CloneWithBlend(self.X, self.Y, GetMask(96));
                } else {
                    data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(86));
                }
            } else if (self != tile1) {
                data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(6));
            } else {
                data[count++] = tile3.CloneWithBlend(self.X, self.Y, GetMask(1));
            }
        }
        
        // bottom right
        tile1 = GetTile(tiles[5], self);
        tile2 = GetTile(tiles[8], self);
        tile3 = GetTile(tiles[7], self);
        
        if (self != tile1 || self != tile2 || self != tile3) {
            if (self == tile1 && self == tile3) {
                data[count++] = tile2.CloneWithBlend(self.X, self.Y, GetMask(83));
            } else if (self != tile1 && self != tile3) {
                if (tile1.Type != tile3.Type) {
                    data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(93));
                    data[count++] = tile3.CloneWithBlend(self.X, self.Y, GetMask(96));
                } else {
                    data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(87));
                }
            } else if (self != tile1) {
                data[count++] = tile1.CloneWithBlend(self.X, self.Y, GetMask(3));
            } else {
                data[count++] = tile3.CloneWithBlend(self.X, self.Y, GetMask(7));
            }
        }

        return count;
    }

    private static MapTile GetTile(MapTile tile, MapTile self) {
        if (tile == null) {
            return self;
        }

        return tile.GroundProperties.BlendPriority > self.GroundProperties.BlendPriority ? tile : self;
    }
    
    private static Vector4 GetMask(int direction) {
        switch (direction) {
            /* left - top left */          case 0: return Masks[Random.Shared.NextRange(0, 2)];
            /* left - bottom left */       case 1: return Masks[Random.Shared.NextRange(4, 6)];
            /* right - top left */         case 2: return Masks[Random.Shared.NextRange(8, 10)];
            /* right - bottom left */      case 3: return Masks[Random.Shared.NextRange(12, 14)];
            /* top - top left */           case 4: return Masks[Random.Shared.NextRange(16, 18)];
            /* top - top right */          case 5: return Masks[Random.Shared.NextRange(20, 22)];
            /* bottom - bottom left */     case 6: return Masks[Random.Shared.NextRange(24, 26)];
            /* bottom - bottom right */    case 7: return Masks[Random.Shared.NextRange(28, 30)];
            /* outer - top left */         case 80: return Masks[32];
            /* outer - top right */        case 81: return Masks[33];
            /* outer - bottom left */      case 82: return Masks[34];
            /* outer - bottom right */     case 83: return Masks[35];
            /* inner - top left */         case 84: return Masks[36];
            /* inner - top right */        case 85: return Masks[37];
            /* inner - bottom left */      case 86: return Masks[38];
            /* inner - bottom right */     case 87: return Masks[39];

            /* inner 1 - top left */       case 90: return Masks[40];
            /* inner 1 - top right */      case 91: return Masks[41];
            /* inner 1 - bottom left */    case 92: return Masks[42];
            /* inner 1 - bottom right */   case 93: return Masks[43];

            /* inner 2 - top left */       case 94: return Masks[44];
            /* inner 2 - top right */      case 95: return Masks[45];
            /* inner 2 - bottom left */    case 96: return Masks[46];
            /* inner 2 - bottom right */   case 97: return Masks[47];

            /* composite - top left */     case 100: return Masks[48];
            /* composite - top right */    case 101: return Masks[49];
            /* composite - bottom left */  case 102: return Masks[50];
            /* composite - bottom right */ case 103: return Masks[51];
            default: throw new Exception("[Blend Mask] direction not supported");
        }
    }
}