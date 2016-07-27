﻿// --------------------------------------------------------------
// VLEUIController.cs is part of the VLAB project.
// Copyright (c) 2016 All Rights Reserved
// Li Alex Zhang fff008@gmail.com
// 6-16-2016
// --------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using VLab;

namespace VLabEnvironment
{
    public class VLEUIController : MonoBehaviour
    {
        public InputField serveraddress;
        public Toggle clientconnect, autoconn;
        public Text autoconntext;
        public VLENetManager netmanager;
        public Canvas canvas;
        public VLEApplicationManager appmanager;

        bool isautoconn, isconnect;
        int autoconncountdown;
        float lastautoconntime;

        public float GetAspectRatio()
        {
            var rootrt = canvas.gameObject.transform as RectTransform;
            return rootrt.rect.width / rootrt.rect.height;
        }

        public void OnRectTransformDimensionsChange()
        {
            if (isconnect)
            {
                netmanager.client.Send(VLMsgType.AspectRatio, new FloatMessage(GetAspectRatio()));
            }
        }

        public void OnToggleClientConnect(bool isconn)
        {
            if (isconn)
            {
                netmanager.networkAddress = serveraddress.text;
                netmanager.StartClient();
            }
            else
            {
                netmanager.StopClient();
                OnClientDisconnect();
            }
        }

        public void OnServerAddressEndEdit(string v)
        {
            appmanager.config[VLECFG.ServerAddress] = v;
        }

        public void OnToggleAutoConnect(bool ison)
        {
            appmanager.config[VLECFG.AutoConnection] = ison;
            ResetAutoConnect();
        }

        public void ResetAutoConnect()
        {
            autoconncountdown = (int)appmanager.config[VLECFG.AutoConnectionTimeOut];
            isautoconn = (bool)appmanager.config[VLECFG.AutoConnection];
            if (!isautoconn)
            {
                autoconntext.text = "Auto Connect OFF";
            }
            autoconn.isOn = isautoconn;
        }

        void Start()
        {
            serveraddress.text = (string)appmanager.config[VLECFG.ServerAddress];
            ResetAutoConnect();
        }

        void Update()
        {
            if (isautoconn && !isconnect)
            {
                if (Time.unscaledTime - lastautoconntime >= 1)
                {
                    autoconncountdown--;
                    if (autoconncountdown > 0)
                    {
                        lastautoconntime = Time.unscaledTime;
                        autoconntext.text = "Auto Connect " + autoconncountdown + "s";
                    }
                    else
                    {
                        clientconnect.isOn = true;
                        clientconnect.onValueChanged.Invoke(true);
                        autoconntext.text = "Connecting ...";
                        isautoconn = false;
                    }
                }
            }
        }

        public void OnClientConnect()
        {
            isconnect = true;
            autoconntext.text = "Connected";
            // since VLabEnvironment is to provide virtual reality environment, we may want to
            // hide cursor and default ui when connected to VLab.
            canvas.enabled = !(bool)appmanager.config[VLECFG.HideUIWhenConnected];
            Cursor.visible = !(bool)appmanager.config[VLECFG.HideCursorWhenConnected];
            // when connected to VLab, we need to make sure that all system resourses
            // VLabEnvironment needed is ready to start experiment.
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            QualitySettings.vSyncCount = (int)appmanager.config[VLECFG.VSyncCount];
            QualitySettings.maxQueuedFrames = (int)appmanager.config[VLECFG.MaxQueuedFrames];
            Time.fixedDeltaTime = (float)appmanager.config[VLECFG.FixedDeltaTime];

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        }

        public void OnClientDisconnect()
        {
            isconnect = false;
            // when disconnected, we should go back to default ui and turn on cursor.
            ResetAutoConnect();
            clientconnect.isOn = false;
            canvas.enabled = true;
            Cursor.visible = true;
            // when disconnect, we can relax and release some system resourses for other process
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.vSyncCount = 1;
            QualitySettings.maxQueuedFrames = 2;
            Time.fixedDeltaTime = 0.02f;

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
        }

    }
}