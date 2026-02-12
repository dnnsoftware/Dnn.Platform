import state from "../stores/store";

export class EventLogSeverity {
     readonly info: EventLogSeverityInfo;
     readonly warning: EventLogSeverityInfo;
     readonly alert: EventLogSeverityInfo;
     readonly critical: EventLogSeverityInfo;

     constructor(){
         this.info = new EventLogSeverityInfo("Info");
         this.warning = new EventLogSeverityInfo("Warning");
         this.alert = new EventLogSeverityInfo("Alert");
         this.critical = new EventLogSeverityInfo("Critical");
     }
}

export class EventLogSeverityInfo{
   readonly eventLogSeverityKey: string;

     get localizedName(){
       console.log(state.resx);
       console.log(`EventLogSeverity_${this.eventLogSeverityKey}`);
         return state.resx[`EventLogSeverity_${this.eventLogSeverityKey}`];
     }

     constructor(eventLogSeverityKey: string = "Info"){
         this.eventLogSeverityKey = eventLogSeverityKey;
     }
}

export const eventLogSeverity = new EventLogSeverity();
