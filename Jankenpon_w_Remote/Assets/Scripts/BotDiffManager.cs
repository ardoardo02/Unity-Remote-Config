using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;
using System;

public class BotDiffManager : MonoBehaviour
{
    [SerializeField] Bot bot;
    [SerializeField] int selectedDifficulty;
    [SerializeField] BotStats[] botDifficulties;

    [Header("Remote Config Parameters:")]
    [SerializeField] bool enableRemoteConfig = false;
    [SerializeField] string diffKey = "Difficulty";

    struct userAttributes{};
    struct appAttributes{};

    IEnumerator Start() {
        yield return new WaitUntil(() => bot.IsReady);

        var newStats = botDifficulties[selectedDifficulty];
        bot.SetStats(newStats, true);

        if(enableRemoteConfig == false)
            yield break;
        
        yield return new WaitUntil(
            () => UnityServices.State == ServicesInitializationState.Initialized
            && AuthenticationService.Instance.IsSignedIn
        );

        RemoteConfigService.Instance.FetchCompleted += OnRemoteConfigFetched;

        RemoteConfigService.Instance.FetchConfigsAsync(
            new userAttributes(), new appAttributes()
        );
    }

    private void OnDestroy() {
        RemoteConfigService.Instance.FetchCompleted -= OnRemoteConfigFetched;
    }

    private void OnRemoteConfigFetched(ConfigResponse response)
    {
        if(RemoteConfigService.Instance.appConfig.HasKey(diffKey) == false){
            Debug.LogWarning($"Diff key: {diffKey} not found on remote config server ");
            return;
        }
        
        switch(response.requestOrigin)
        {
            case ConfigOrigin.Default:
            case ConfigOrigin.Cached:
                break;
            case ConfigOrigin.Remote:
                selectedDifficulty = RemoteConfigService.Instance.appConfig.GetInt(diffKey);
                selectedDifficulty =  Mathf.Clamp(selectedDifficulty, 0, botDifficulties.Length);
                
                var newStats = botDifficulties[selectedDifficulty];
                bot.SetStats(newStats, true);
                break;
        }
    }
}
