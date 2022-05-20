using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class IPConnectionInput : MonoBehaviour
{
    public UnityTransport transport;
    public TMP_InputField ipInput;
    public TMP_InputField portInput;

    private void Update()
    {
        transport.ConnectionData.Address = ipInput.text;
        transport.ConnectionData.Port = ushort.Parse(portInput.text);
    }

}
