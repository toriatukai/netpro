using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;

using UnityEngine.SceneManagement;



public class SkillRetention : MonoBehaviour

{

    public static SkillRetention Instance { get; private set; }



    private Dictionary<ulong, SkillType> selectedSkills = new();

    private HashSet<ulong> readyClients = new();



    private void Awake()

    {

        if (Instance != null && Instance != this)

        {

            Destroy(gameObject); // 二重生成防止

            return;

        }



        Instance = this;

        DontDestroyOnLoad(gameObject); // シーン遷移時に破棄しない

    }



    public void SetSkillForClient(ulong clientId, SkillType skill)

    {

        selectedSkills[clientId] = skill;

        Debug.Log($"スキル設定: clientId={clientId}, skill={skill}");

    }



    public SkillType GetSkillForClient(ulong clientId)

    {

        return selectedSkills.TryGetValue(clientId, out var skill) ? skill : SkillType.None;

    }



    [ServerRpc(RequireOwnership = false)]

    public void SubmitReadyServerRpc(ulong clientId)

    {

        Debug.Log($"[ServerRpc] Client {clientId} ready received on server.");



        readyClients.Add(clientId);

        Debug.Log($"[ServerRpc] Current readyClients count: {readyClients.Count}");



        if (readyClients.Count >= 2)

        {

            Debug.Log("All players ready. Loading MainGame scene...");

            NetworkManager.Singleton.SceneManager.LoadScene("MainGame", LoadSceneMode.Single);

        }

    }

}