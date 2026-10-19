using System.Collections;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    [SerializeField] private MechaTestProto[] playerMechas = new MechaTestProto[3];
    [SerializeField] private EnemyData[] enemyDatas = new EnemyData[3];
    
    [SerializeField] private float targetSwitchInterval = 5f;
    
    public FightEntity[] PlayerTeam { get; private set; } = new FightEntity[3];
    public FightEntity[] EnemyTeam { get; private set; } = new FightEntity[3];

    private int currentTargetEnemyIndex;
    private int currentTargetPlayerIndex;

    private bool isBattleActive;

    private void Start()
    {
        InitFight();
        StartBattle();
    }

    public void InitFight()
    {
        InitPlayerTeam();
        InitEnemyTeam();

        SelectRandomTarget(EnemyTeam, ref currentTargetEnemyIndex);
        SelectRandomTarget(PlayerTeam, ref currentTargetPlayerIndex);
    }

    private void InitPlayerTeam()
    {
        for (int i = 0; i < playerMechas.Length; i++)
        {
            if (playerMechas[i]?.BaseData != null)
            {
                if (PlayerTeam[i] != null) 
                    PlayerTeam[i].OnDeath -= HandlePlayerDeath;

                var runtimeData = new EntityRuntimeData(playerMechas[i].BaseData, playerMechas[i].Modules);
                PlayerTeam[i] = new FightEntity(runtimeData);
                
                PlayerTeam[i].OnDeath += HandlePlayerDeath;
            }
        }
    }

    private void InitEnemyTeam()
    {
        for (int i = 0; i < enemyDatas.Length; i++)
        {
            if (enemyDatas[i] != null)
            {
                if (EnemyTeam[i] != null) 
                    EnemyTeam[i].OnDeath -= HandleEnemyDeath;

                EnemyTeam[i] = new FightEntity(new EntityRuntimeData(enemyDatas[i]));
                EnemyTeam[i].OnDeath += HandleEnemyDeath;
            }
        }
    }

    private void HandlePlayerDeath(FightEntity deadEntity)
    {
        if (PlayerTeam[currentTargetPlayerIndex] == deadEntity)
        {
            SelectRandomTarget(PlayerTeam, ref currentTargetPlayerIndex);
        }
        
        CheckBattleStatus();
    }

    private void HandleEnemyDeath(FightEntity deadEntity)
    {
        for (int i = 0; i < EnemyTeam.Length; i++)
        {
            if (EnemyTeam[i] == deadEntity)
            {
                if (enemyVisuals[i] != null) 
                    enemyVisuals[i].SetActive(false);
                break;
            }
        }

        if (EnemyTeam[currentTargetEnemyIndex] == deadEntity)
        {
            if (isManualFocusActive)
            {
                isManualFocusActive = false;
                if (focusIndicator != null) focusIndicator.SetActive(false);
            }

            SelectRandomTarget(EnemyTeam, ref currentTargetEnemyIndex);
        }
    
        CheckBattleStatus();
    }

    public void StartBattle()
    {
        isBattleActive = true;
        
        StartTeamCoroutines(PlayerTeam, true);
        StartTeamCoroutines(EnemyTeam, false);

        StartCoroutine(TargetSwitchRoutine());
    }

    private IEnumerator TargetSwitchRoutine()
    {
        while (isBattleActive)
        {
            yield return new WaitForSeconds(targetSwitchInterval);

            if (!isBattleActive) yield break;

            SelectRandomTarget(PlayerTeam, ref currentTargetPlayerIndex);
            
            if (!isManualFocusActive)
            {
                SelectRandomTarget(EnemyTeam, ref currentTargetEnemyIndex);
            }
        }
    }

    private void SelectRandomTarget(FightEntity[] team, ref int currentTargetIndex)
    {
        int livingCount = 0;
        for (int i = 0; i < team.Length; i++)
        {
            if (team[i] != null && !team[i].IsDead) 
                livingCount++;
        }

        if (livingCount == 0) return;

        int randomPick = Random.Range(0, livingCount);
        int currentAliveFound = 0;

        for (int i = 0; i < team.Length; i++)
        {
            if (team[i] != null && !team[i].IsDead)
            {
                if (currentAliveFound == randomPick)
                {
                    currentTargetIndex = i;
                    return;
                }
                currentAliveFound++;
            }
        }
    }

    private void StartTeamCoroutines(FightEntity[] team, bool isPlayerTeam)
    {
        foreach (var entity in team)
        {
            if (entity != null)
                StartCoroutine(EntityAttackRoutine(entity, isPlayerTeam));
        }
    }

    private IEnumerator EntityAttackRoutine(FightEntity attacker, bool isPlayerTeam)
    {
        while (isBattleActive && !attacker.IsDead)
        {
            yield return new WaitForSeconds(100f / Mathf.Max(1, attacker.Speed));

            if (!isBattleActive || attacker.IsDead) yield break;

            FightEntity target = isPlayerTeam 
                ? EnemyTeam[currentTargetEnemyIndex] 
                : PlayerTeam[currentTargetPlayerIndex];

            if (target != null && !target.IsDead)
            {
                attacker.AttackTarget(target);
            }
        }
    }

    private void CheckBattleStatus()
    {
        bool playerAlive = IsTeamAlive(PlayerTeam);
        bool enemyAlive = IsTeamAlive(EnemyTeam);

        if (!playerAlive || !enemyAlive)
        {
            isBattleActive = false;
            StopAllCoroutines();

            if (!playerAlive && !enemyAlive) Debug.Log("ÉGALITÉ");
            else if (playerAlive) Debug.Log("VICTOIRE JOUEUR");
            else Debug.Log("DÉFAITE JOUEUR");
        }
    }

    private bool IsTeamAlive(FightEntity[] team)
    {
        foreach (var entity in team)
        {
            if (entity != null && !entity.IsDead) return true;
        }
        return false;
    }
    
    [SerializeField] private GameObject focusIndicator;
    [SerializeField] private Transform[] focusPositions = new Transform[3];
    
    private bool isManualFocusActive = false;

    public void SetManualEnemyTarget(int index)
    {
        if (EnemyTeam[index] == null || EnemyTeam[index].IsDead) return;

        isManualFocusActive = true;
        currentTargetEnemyIndex = index;

        if (focusIndicator != null && focusPositions.Length > index)
        {
            focusIndicator.transform.position = focusPositions[index].position;
            focusIndicator.SetActive(true);
        }
    }
    
    [Header("Visuels Ennemis")]
    [SerializeField] private GameObject[] enemyVisuals = new GameObject[3];
    
    
}