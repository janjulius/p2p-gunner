using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class ConnectInterfaceHandler : MonoBehaviour
    {
        public TextMeshProUGUI ip;
        public TextMeshProUGUI port;
        public TextMeshProUGUI maxPlayers;
        public TextMeshProUGUI pwd;

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
            NetworkManager.InitGame(ip.text, p, pwd.text, -1, false);
        }

        public void Host()
        {
            int p = 9050;
            if (!Int32.TryParse(port.text, out p))
                p = 9050;
            int maxp = 10;
            if (!Int32.TryParse(maxPlayers.text, out maxp))
                maxp = 10;
            NetworkManager.InitGame("", p, pwd.text, maxp, true);
        }

    }
}
