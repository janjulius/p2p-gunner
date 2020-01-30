using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ConnectInterfaceHandler : MonoBehaviour
    {
        public TextMeshProUGUI ip;
        public TextMeshProUGUI port;

        public Slider sliderr, sliderg, sliderb;

        public NetworkManager NetworkManager;

        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public void Connect()
        {
            int p = 9050;
            if (!Int32.TryParse(port.text, out p))
                p = 9050;
            NetworkManager.InitGame(ip.text, p, false);
        }

        public void Host()
        {
            int p = 9050;
            if (!Int32.TryParse(port.text, out p))
                p = 9050;
            NetworkManager.InitGame("", p, true);
        }

        public void SendColor()
        {
            print(sliderr.value);
            NetworkManager.SendColorUpdate(new Color(sliderr.value, sliderg.value, sliderb.value));
        }

    }
}
