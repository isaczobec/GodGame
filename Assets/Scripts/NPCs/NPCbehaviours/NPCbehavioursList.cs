
public enum NPCBehaviourType {
    MercenaryWarrior,
    BasicMelee,
    PassiveMovement,
    AggresiveMovement
}

public class NPCBehavioursList {
    public static NPCBehaviour GetNPCbehaviour(NPCBehaviourType type) {
        switch (type) {
            case NPCBehaviourType.MercenaryWarrior:
                return new NPCBehaviourMercenaryWarrior();
            case NPCBehaviourType.BasicMelee:
                return new NPCBehaviourBasicMelee();
            default:
                return null;
        }

    }

}