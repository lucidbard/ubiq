﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Networking;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;
using Avatar = Ubiq.Avatars.Avatar;
/// <summary>
/// Puts the current object on a different layer and hides it.
/// Hide layer: 8;
/// </summary>
public class ObjectHider : MonoBehaviour, INetworkComponent, ILayer
{
    private NetworkContext context;
    private NetworkScene scene;
    private RoomClient roomClient;

    private int hideLayer = 8;
    private int defaultLayer = 0; // default
    private int currentLayer;
    private bool needsUpdate = false;
    private Transform[] childTransforms;
    private Avatar avatar;
    private NetworkSpawner spawner;

    public struct Message
    {
        public int layer;
    }

    public void UpdateLayer(int layer)
    {
        currentLayer = layer;
        needsUpdate = true;
    }

    public int GetCurrentLayer()
    {
        return currentLayer;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        Debug.Log(Time.unscaledTime + " Process Message: " + message.length + " " + System.String.Join(" ", message.bytes));
        Debug.Log(Time.unscaledTime + " Process Message: " + message.ToString());

        Message msg = message.FromJson<Message>();
        Debug.Log("Remote: SetLayer " + msg.layer);
        SetLayer(msg.layer);
    }
// Awake instead of Start because objects are hidden immediately after creation.
// Should be safe to use because a replay usually only takes place in an existing scene that has the context already set.
    void Awake()
    {
        Debug.Log("ObjectHider Awake()");
        scene = NetworkScene.FindNetworkScene(this);
        context = scene.RegisterComponent(this);
        roomClient = NetworkScene.FindNetworkScene(this).GetComponent<RoomClient>();
        //roomClient.OnPeerAdded.AddListener(OnPeerAdded);
        //roomClient.OnPeerRemoved.AddListener(OnPeerRemoved);
        childTransforms = GetComponentsInChildren<Transform>();
    }

    void Start()
    {
        if (context == null)
        {
            context = NetworkScene.Register(this);
        }

        avatar = GetComponent<Avatar>();
        spawner = NetworkSpawner.FindNetworkSpawner(context.scene);
    }

    public void OnPeerAdded(IPeer peer)
    {
        if (scene.recorder != null && scene.recorder.IsRecording())
        {
            //Debug.Log("UUID:" + peer.UUID + " " + roomClient.Me.UUID);
            if (peer.UUID == roomClient.Me.UUID)
            {
                Debug.Log("Set layer for joining/leaving avatar  (OnPeerUpdated)");
                SetNetworkedObjectLayer(0);
            }
            else
            {
                Debug.Log("Does it even get called at the right time?");
            }
        }
    }

    public void OnPeerRemoved(IPeer peer)
    {
        if (scene.recorder != null && scene.recorder.IsRecording())
        {
            if (peer.UUID == roomClient.Me.UUID)
            {
                Debug.Log("Set layer for joining/leaving avatar (OnPeerRemoved)");
                SetNetworkedObjectLayer(8);
            }
        }
    }


    public void SetLayer(int layer) // not networked
    {
        currentLayer = layer;
        this.gameObject.layer = layer;
        foreach (Transform child in childTransforms)
        {
            child.gameObject.layer = layer;
        }
    }
    // only call locally
    public void NetworkedShow()
    {
        Debug.Log("Show (layer) " + defaultLayer);
        context.SendJson(new Message() { layer = defaultLayer });
        SetLayer(defaultLayer);
    }

    // only call locally
    public void NetworkedHide()
    {
        Debug.Log("Hide (layer) " + hideLayer);
        context.SendJson(new Message() { layer = hideLayer });
        SetLayer(hideLayer);
    }

    public void SetNetworkedObjectLayer(int layer)
    {

        if (layer == defaultLayer)
        {
            // for actual peer avatars this method won't do anything because the peer avatars are in the AvatarManager and not in the NetworkSpawner
            if (avatar != null)
            {
                spawner.UpdateProperties(avatar.Id, "UpdateVisibility", true);
            }
            NetworkedShow();
        }
        else if (layer == hideLayer)
        {
            if (avatar != null)
            {
                spawner.UpdateProperties(avatar.Id, "UpdateVisibility", false);
            }
            NetworkedHide();
        }
        else
        {
            Debug.Log("Layer " + layer + " has no specified use here.");
        }
    }

    //void Update()
    //{
    //    if (needsUpdate && avatar.IsLocal)
    //    {
    //        Debug.Log("Set layer for joining/leaving avatar");
    //        SetNetworkedObjectLayer(currentLayer);
    //        needsUpdate = false;
    //    }
    //}
}
