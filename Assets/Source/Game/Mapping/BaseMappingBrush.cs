using Assets.Source.Game.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Mapping
{
    public abstract class BaseMappingBrush
    {
        protected List<(Dictionary<(int, int), int[]>, Dictionary<(int, int), int[]>)> _actionQueue;

        public abstract void Apply();

        protected virtual void SetVertexHeight(Dictionary<(int, int), int[]> oldHeigts, int x, int z, int height)
        {
            oldHeigts.Add((x, z), new int[] { GameMap.Instance.SetTileCornerHeight(x, z, height) });
        }

        protected virtual void IncreaseVertexHeight(Dictionary<(int, int), int[]> oldHeights, int x, int z, int height)
        {
            oldHeights.Add((x, z), new int[] { GameMap.Instance.IncreaseTileCornerHeight(x, z, height) });
        }

        protected virtual void SetTileHeight(Dictionary<(int, int), int[]> oldHeights, int x, int z, int height)
        {
            oldHeights.Add((x, z), GameMap.Instance.SetTileHeight(x, z, height));
        }

        protected virtual void IncreaseTileHeight(Dictionary<(int, int), int[]> oldHeights, int x, int z, int height)
        {
            oldHeights.Add((x, z), GameMap.Instance.IncreaseTileHeight(x, z, height));
        }

        protected virtual void SetTileId(Dictionary<(int, int), int> oldTiles, int x, int z, short tileId)
        {
            oldTiles.Add((x, z), GameMap.Instance.SetTileId(x, z, tileId));
        }
    }
}
