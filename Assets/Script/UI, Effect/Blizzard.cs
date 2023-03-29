using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blizzard : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Chaser")) {
            var agent = collision.gameObject.GetComponent<AIAgent>();
            agent.dirIndicator.GetComponent<SpriteRenderer>().color = Color.gray;
            agent.dirIndicator2.GetComponent<SpriteRenderer>().color = Color.gray;

            agent.gameObject.tag = "Frozen";
            agent.gameObject.layer = LayerMask.NameToLayer("Frozen");

            agent.isKnockedOut = true;

            agent.anim.SetBool("isFrozen", true);
            agent.unfreezeCoroutine = agent.StartCoroutine(agent.UnfreezeAfterDelay());
        }
    }
}
