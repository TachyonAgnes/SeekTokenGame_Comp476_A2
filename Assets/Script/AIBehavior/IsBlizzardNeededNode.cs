using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IsBlizzardNeededNode : ActionNode {
    private AIAgent agent;
    private float wallDistance = 1.5f;
    private float enemyDistance = 6f;                
    private int bombInTotal = 2;
    private float lastThrowTime; 
    public float throwCooldown = 1.0f; 


    public IsBlizzardNeededNode(AIAgent agent) {
        this.agent = agent;
    }
    public override NodeStatus Execute() {
        if(bombInTotal > 0) {
            var mask = LayerMask.GetMask("Obstacle");
            bool isAnimationPlaying = agent.anim.GetCurrentAnimatorStateInfo(0).IsName("throwing");
            bool isEnemyFrozen = agent.target.layer == LayerMask.NameToLayer("Frozen");

            bool isCooldownOver = Time.time - lastThrowTime >= throwCooldown;

            if (isCooldownOver && !isEnemyFrozen && !isAnimationPlaying && IsAgentCornered(agent.gameObject, agent.target, mask, wallDistance, enemyDistance) ) {
                agent.anim.SetTrigger("throwing");
                lastThrowTime = Time.time;
                bombInTotal--;
            }
        }
        return NodeStatus.SUCCESS;
    }


    public bool IsAgentCornered(GameObject agent, GameObject enemy, LayerMask wallLayerMask, float wallDistanceThreshold, float enemyDistanceThreshold) {
        // check distance between agent and wall
        bool isNearWall = Physics.CheckSphere(agent.transform.position, wallDistanceThreshold, wallLayerMask);

        // calculate distance to enemy
        float distanceToEnemy = Vector3.Distance(agent.transform.position, enemy.transform.position);

        // check if the agent is cornered
        if (isNearWall && distanceToEnemy < enemyDistanceThreshold) {
            return true;
        }

        return false;
    }

}
