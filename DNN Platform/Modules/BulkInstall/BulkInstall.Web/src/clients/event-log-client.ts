import { DnnServicesFramework } from "@dnncommunity/dnn-elements";
import type { Event } from "../components/tabs/bulk-install-logs/bulk-install-logs.model";
import { EventLogSeverityInfo, eventLogSeverity } from "../enums/EventLogSeverity";

export class EventLogClient
{
    private readonly sf: DnnServicesFramework;
    private readonly requestUrl: string;

    constructor(moduleId: number) {
        this.sf = new DnnServicesFramework(moduleId);
        this.requestUrl = this.sf.getServiceRoot("BulkInstall") + "EventLog/";
    }

    public async browse(): Promise<BrowseResponse>
    {
        const response = await fetch(
            `${this.requestUrl}Browse`,
            {
                headers: this.sf.getModuleHeaders(),
            });
        const responseBody = await response.json() as ResponseBody;
        const browseResponse = {
          Data: responseBody.Data.map(e => EventLogClient.toEvent(e)),
          Pagination: responseBody.Pagination,
        };

      return browseResponse;
    }

    private static toSeverity(severity: EventLogSeverityResponse): EventLogSeverityInfo {
      switch (severity) {
        case EventLogSeverityResponse.Info:
          return eventLogSeverity.info;
        case EventLogSeverityResponse.Warning:
          return eventLogSeverity.warning;
        case EventLogSeverityResponse.Alert:
          return eventLogSeverity.alert;
        case EventLogSeverityResponse.Critical:
          return eventLogSeverity.critical;
        default:
          throw new Error(`Unknown severity: ${severity}`);
      }
    }

    private static toEvent(eventLog: EventLogResponse): Event {
      return {
        date: new Date(eventLog.Date),
        message: eventLog.Message,
        type: eventLog.EventType,
        severity: EventLogClient.toSeverity(eventLog.Severity),
      };
    }
}

export interface BrowseResponse {
  Data: Event[]
  Pagination: Pagination
}

export interface Pagination {
  Records: number
  Pages: number
  CurrentPage: number
  Navigation: {
    Previous?: string
    Next?: string
  }
}

interface ResponseBody {
  Data: EventLogResponse[]
  Pagination: Pagination
}

interface EventLogResponse {
  EventLogID: number
  Date: string
  EventType: string
  Severity: EventLogSeverityResponse
  Message: string
  StackTrace: string
}

enum EventLogSeverityResponse {
  Info = 0,
  Warning = 1,
  Alert = 2,
  Critical = 3,
}
