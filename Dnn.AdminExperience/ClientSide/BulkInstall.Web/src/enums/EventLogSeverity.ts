import state from '../stores/store';

export class EventLogSeverity {
  readonly info: EventLogSeverityInfo;
  readonly warning: EventLogSeverityInfo;
  readonly alert: EventLogSeverityInfo;
  readonly critical: EventLogSeverityInfo;

  constructor() {
    this.info = new EventLogSeverityInfo('Info');
    this.warning = new EventLogSeverityInfo('Warning');
    this.alert = new EventLogSeverityInfo('Alert');
    this.critical = new EventLogSeverityInfo('Critical');
  }

  public fromKey(eventLogSeverityKey: string) {
    switch (eventLogSeverityKey) {
      case this.info.eventLogSeverityKey:
        return this.info;
      case this.warning.eventLogSeverityKey:
        return this.warning;
      case this.alert.eventLogSeverityKey:
        return this.alert;
      case this.critical.eventLogSeverityKey:
        return this.critical;
      default:
        throw new Error(`Invalid EventLogSeverity key: ${eventLogSeverityKey}`);
    }
  }
}

export class EventLogSeverityInfo {
  readonly eventLogSeverityKey: string;

  get localizedName() : string {
    const key = `EventLogSeverity_${this.eventLogSeverityKey}`;
    return state.resx[key] || this.eventLogSeverityKey;
  }

  constructor(eventLogSeverityKey: string = 'Info') {
    this.eventLogSeverityKey = eventLogSeverityKey;
  }
}

export const eventLogSeverity = new EventLogSeverity();
