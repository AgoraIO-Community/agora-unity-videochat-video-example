using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;


public class AgoraEngine : MonoBehaviour
{
    public string appID;
    public InputField channelNameInputField;

    private IRtcEngine mRtcEngine;
    private List<GameObject> playerVideoList;

    const float VIDEO_X_POS_OFFSET = 20;

    void Start()
    {
        if(mRtcEngine != null)
        {
            IRtcEngine.Destroy();
        }

        playerVideoList = new List<GameObject>();

        mRtcEngine = IRtcEngine.GetEngine(appID);

        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccessHandler;
        mRtcEngine.OnUserJoined = OnUserJoinedHandler;
        mRtcEngine.OnLeaveChannel = OnLeaveChannelHandler;
        mRtcEngine.OnUserOffline = OnUserOfflineHandler;

        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();
    }

    private void OnApplicationQuit()
    {
        if (mRtcEngine != null)
        {
            mRtcEngine.LeaveChannel();
            mRtcEngine = null;
            IRtcEngine.Destroy();
        }
    }

    public void Button_JoinChannel()
    {
        mRtcEngine.JoinChannel(channelNameInputField.text);
    }

    public void Button_LeaveChannel()
    {
        mRtcEngine.LeaveChannel();
    }
    
    private void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        CreateUserVideoSurface(uid, true);
    }

    private void OnUserJoinedHandler(uint uid, int elapsed)
    {
        CreateUserVideoSurface(uid, false);
    }

    private void OnLeaveChannelHandler(RtcStats stats)
    {
        foreach(GameObject player in playerVideoList)
        {
            Destroy(player);
        }

        playerVideoList.Clear();
    }

    private void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        RemoveUserVideoSurface(uid);
    }

    private void CreateUserVideoSurface(uint uid, bool isLocalUser)
    {
        // Avoid duplicating Local player VideoSurface image plane.
        for (int i = 0; i < playerVideoList.Count; i++)
        {
            if (playerVideoList[i].name == uid.ToString())
            {
                Debug.LogWarning("Attempting to duplicate videosurface for user: " + uid);
                return;
            }
        }

        GameObject newUserVideo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        newUserVideo.name = uid.ToString();
        playerVideoList.Add(newUserVideo);

        VideoSurface newVideoSurface = newUserVideo.AddComponent<VideoSurface>();

        // set up transform
        newUserVideo.transform.Rotate(-90f, 0.0f, 0.0f);
        float xPos = Random.Range(-VIDEO_X_POS_OFFSET, VIDEO_X_POS_OFFSET);
        newUserVideo.transform.position = new Vector3(xPos, 5, 15f);

        if(isLocalUser == false)
        {
            newVideoSurface.SetForUser(uid);
        }
        newVideoSurface.SetGameFps(30);
    }

    private void RemoveUserVideoSurface(uint deletedUID)
    {
        foreach (GameObject player in playerVideoList)
        {
            if (player.name == deletedUID.ToString())
            {
                playerVideoList.Remove(player);
                Destroy(player.gameObject);
                break;
            }
        }
    }
}