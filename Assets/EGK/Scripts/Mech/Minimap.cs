using EvanGameKits.Entity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

public class Minimap : MonoBehaviour
{
    private PositionConstraint constraint;
    private void Start()
    {
        constraint = GetComponent<PositionConstraint>();
        Player.onPlayerChange += assignPlayer;
        if (Player.ActivePlayer != null) assignPlayer(Player.ActivePlayer);
    }

    private void assignPlayer(Player activePlayer)
    {
        if (constraint == null) return;
        List<ConstraintSource> sources = new List<ConstraintSource>();
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = activePlayer.transform;
        source.weight = 1f;
        

        sources.Add(source); 
        constraint.SetSources(
            sources
            );
    }
}
