using UnityEngine;
namespace LightBringer.Tools
{

    public static class LayerTools
    {
        public static void recSetLayer(GameObject go, string from, string to)
        {
            if (go.layer == LayerMask.NameToLayer(from))
            {
                go.layer = LayerMask.NameToLayer(to);
            }

            foreach (Transform child in go.transform)
            {
                recSetLayer(child.gameObject, from, to);
            }
        }
    }

}
