
public enum NPCBehaviourType {
    AggresiveFighting,
    PassiveFighting,
    PassiveMovement,
    AggresiveMovement
}

public class NPCBehavioursList {
    public static NPCBehaviour GetNPCbehaviour(NPCBehaviourType type) {
        switch (type) {
            case NPCBehaviourType.AggresiveFighting:
                return new NPCBehaviourAggresiveFighting();
            default:
                return null;
        }

    }

}