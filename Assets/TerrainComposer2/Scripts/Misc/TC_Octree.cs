using UnityEngine;
using System.Collections;


namespace TerrainComposer2
{
    public class Octree
    {
        public Cell cell;
        public int maxLevels;
        
        public class MaxCell
        {

        }

        public class Cell
        {
            public Cell mainParent;
            public Cell parent;
            public Cell[] cells;
            public bool[] cellsUsed;

            public Bounds bounds;
            public int cellIndex;
            public int cellCount;
            public int level;

            public Cell(Cell parent, int cellIndex, Bounds bounds)
            {
                mainParent = parent.mainParent;
                this.parent = parent;
                this.cellIndex = cellIndex;
                this.bounds = bounds;
                level = parent.level + 1;

                
            }

            byte AddCell(Vector3 position)
            {
                Vector3 localPos = position - bounds.min;

                int x = (int)(localPos.x / bounds.extents.x);
                int y = (int)(localPos.y / bounds.extents.y);
                int z = (int)(localPos.z / bounds.extents.z);

                byte index = (byte)(x + (y * 4) + (z * 2));

                if (cells == null) { cells = new Cell[8]; cellsUsed = new bool[8]; }

                // Reporter.Log("index "+index+" position "+localPos+" x: "+x+" y: "+y+" z: "+z+" extents "+bounds.extents);

                if (!cellsUsed[index])
                {
                    Bounds subBounds = new Bounds(new Vector3(bounds.min.x + (bounds.extents.x * (x + 0.5f)), bounds.min.y + (bounds.extents.y * (y + 0.5f)), bounds.min.z + (bounds.extents.z * (z + 0.5f))), bounds.extents);

                    cells[index] = new Cell(this, index, subBounds);
                    cellsUsed[index] = true;
                    ++cellCount;
                }
                return index;
            } 
        }


    }
}
