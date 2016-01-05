﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;   
using System;
using System.Text;
using System.Net;
using System.IO;

public class PlayerRegisterRule : WebServerRule
{

    [Serializable]
    public struct HtmlKeyValue
    {
        [SerializeField]
        private string key;
        public string Key
        {
            get { return key; }
        }

        [SerializeField]
        private string value;
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public HtmlKeyValue(string key, string value)
        {
            this.key = key;
            this.value = value;
        }


    }

#if UNITY_STANDALONE || UNITY_EDITOR

    protected virtual string ModifyHtml(string html)
    {
        return html;
    }

    protected override IEnumerator OnRequest(HttpListenerContext context)
    {
        string dataString = "";

        HttpListenerRequest request = context.Request;
        StreamReader reader = new StreamReader(request.InputStream);
        string username = reader.ReadToEnd();
        username = username.ToLower().Trim();
        //JSONWrapper j = new JSONWrapper(username);

        if (playerRegister.ContainsKey(username))
        {
            dataString = "Player found!";
        }
        else if (!playerRegister.ContainsKey(username) && playerRegister.Count < 5)
        {
            GameObject newController = Instantiate<GameObject>(controllerPrefab);
            CommandPanel newControllerCP = newController.GetComponent<CommandPanel>();
            newControllerCP.PlayerName = username;
            playerRegister.Add(username, newControllerCP);
            dataString = "Player not found; space available, adding " + username + " to the register!";
        }
        else
        {
            dataString = "Maximum player count reached, not accepting new players!";
        }

        Debug.Log(dataString);

        byte[] data = Encoding.ASCII.GetBytes(dataString);

        yield return null;

        HttpListenerResponse response = context.Response;

        response.ContentType = "text/plain";

        Stream responseStream = response.OutputStream;

        int count = data.Length;
        int i = 0;
        while (i < count)
        {
            if (i != 0)
                yield return null;

            int writeLength = Math.Min((int)writeStaggerCount, count - i);
            responseStream.Write(data, i, writeLength);
            i += writeLength;
        }
    }

#endif

    private static Dictionary<String, CommandPanel> playerRegister = new Dictionary<String, CommandPanel>();
    public static Dictionary<String, CommandPanel> PlayerRegister
    {
        get { return playerRegister; }
    }

    [SerializeField]
    private GameObject controllerPrefab;

    [SerializeField]
    private HtmlKeyValue[] substitutions;

    [SerializeField]
    [Tooltip("How many bytes to write before waiting a frame to continue.")]
    private uint writeStaggerCount = 4096;
}