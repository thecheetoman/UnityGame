using System;
using System.Text;
using AOT;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
    protected static bool s_EverInitialized;
    protected static SteamManager s_instance;

    protected bool m_bInitialized;
    protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

    protected static SteamManager Instance
    {
        get
        {
            if (s_instance == null)
                return new GameObject("SteamManager").AddComponent<SteamManager>();

            return s_instance;
        }
    }

    public static bool Initialized => Instance.m_bInitialized;

    [MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
    protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitOnPlayMode()
    {
        s_EverInitialized = false;
        s_instance = null;
    }

    protected virtual void Awake()
    {
        if (s_instance != this && s_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        s_instance = this;
        DontDestroyOnLoad(gameObject);

        // Skip Steam entirely if DLL is missing
        try
        {
            // Disable RestartAppIfNecessary to prevent quitting
            // if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid)) { Application.Quit(); return; }
        }
        catch
        {
            Debug.LogWarning("[Steamworks.NET] Steam DLL missing — running without Steam.");
            m_bInitialized = false;
            return;
        }

        try
        {
            m_bInitialized = SteamAPI.Init();
        }
        catch
        {
            Debug.LogWarning("[Steamworks.NET] SteamAPI.Init failed — running without Steam.");
            m_bInitialized = false;
        }

        if (m_bInitialized)
            s_EverInitialized = true;
    }

    protected virtual void OnEnable()
    {
        if (s_instance == null)
            s_instance = this;

        if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
        {
            m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }
    }

    protected virtual void OnDestroy()
    {
        if (s_instance != this)
            return;

        s_instance = null;

        if (m_bInitialized)
            SteamAPI.Shutdown();
    }

    protected virtual void Update()
    {
        if (m_bInitialized)
            SteamAPI.RunCallbacks();
    }
}