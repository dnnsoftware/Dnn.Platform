import state from "../stores/store";

export class SessionStatus {
     readonly notStarted: SessionStatusInfo;
     readonly inProgress: SessionStatusInfo;
     readonly complete: SessionStatusInfo;

     constructor(){
         this.notStarted = new SessionStatusInfo("NotStarted");
         this.inProgress = new SessionStatusInfo("InProgress");
         this.complete = new SessionStatusInfo("Complete");
     }
}

export class SessionStatusInfo{
   readonly sessionStatusKey: string;

     get localizedName(){
         return state.resx[`SessionStatus_${this.sessionStatusKey}`];
     }

     constructor(eventLogSeverityKey: string = "NotStarted"){
         this.sessionStatusKey = eventLogSeverityKey;
     }
}

export const sessionStatus = new SessionStatus();
