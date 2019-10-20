using System.Collections.Generic;
using UnityEngine;
namespace LightBringer.Player
{
    [RequireComponent (typeof (PlayerMotor))]
    public class LayerManager : MonoBehaviour
    {
        public static readonly string[] layerNames = { "Player", "Immaterial", "NoCollision" };

        private PlayerLayer currentLayer;

        public enum PlayerLayer
        {
            Player = 0,
            Immaterial = 1,
            NoCollision = 2
        }

        private Dictionary<object, PlayerLayer> calledLayers;

        private void Start ()
        {
            Init ();
        }

        public void Init ()
        {
            calledLayers = new Dictionary<object, PlayerLayer> ();
            currentLayer = PlayerLayer.Player;
        }

        public void CallLayer (PlayerLayer layer, object caller)
        {
            calledLayers.Add (caller, layer);
            ComputeLayer ();
        }

        public void DiscardLayer (object caller)
        {
            calledLayers.Remove (caller);
            ComputeLayer ();
        }

        private void ComputeLayer ()
        {
            PlayerLayer newLayer = PlayerLayer.Player;

            foreach (PlayerLayer layer in calledLayers.Values)
            {
                if ((int)layer > (int)newLayer)
                {
                    newLayer = layer;
                }
            }

            if (newLayer != currentLayer)
            {
                recSetLayer (gameObject, layerNames[(int)currentLayer], layerNames[(int)newLayer]);
                currentLayer = newLayer;
            }
        }

        private void recSetLayer (GameObject go, string from, string to)
        {
            if (go.layer == LayerMask.NameToLayer (from))
            {
                go.layer = LayerMask.NameToLayer (to);
            }

            foreach (Transform child in go.transform)
            {
                recSetLayer (child.gameObject, from, to);
            }
        }
    }
}