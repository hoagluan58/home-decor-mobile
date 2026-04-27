using Redcode.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class BotOrderNodePattern : MonoBehaviour
    {
        [SerializeField] private BotOrderNode[] _botOrderNodes;

        private List<BotOrderData> _data;

        public void SetData(List<BotOrderData> data)
        {
            _data = data;
            gameObject.SetActive(true);
            _botOrderNodes.ForEach(x => x.gameObject.SetActive(false));

            for (int i = 0; i < _data.Count; i++)
            {
                _botOrderNodes[i].SetData(_data[i]);
            }
        }

        public BotOrderNode GetBotOrderNodeBaseOnData(int orderId)
        {
            foreach (var botOrderNode in _botOrderNodes)
            {
                if (botOrderNode.OrderId == orderId)
                {
                    return botOrderNode;
                }
            }
            return null;
        }
    }
}
