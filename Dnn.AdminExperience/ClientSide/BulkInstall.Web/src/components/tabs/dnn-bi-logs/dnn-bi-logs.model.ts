import { EventLogSeverityInfo } from '../../../enums/EventLogSeverity';

export interface Event {
  date: Date;
  type: string;
  message: string;
  severity: EventLogSeverityInfo;
}
