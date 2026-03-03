using UnityEngine;

namespace SFS.Parts.Modules
{
    public class BuildColor : ColorModule, I_InitializePartModule
    {
        public Color buildColor = Color.white;

        int I_InitializePartModule.Priority => -1;

        public override Color GetColor()
        {
            return buildColor;
        }

        void I_InitializePartModule.Initialize()
        {
            // Log the actual color value so we can see what's happening
            Debug.Log($"[BuildColor] {gameObject.name} buildColor = {buildColor}");
            
            Part part = GetComponentInParent<Part>();
            if (part != null)
            {
                part.RegenerateMesh();
            }
        }
    }
}