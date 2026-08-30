import type { Event } from '../components/tabs/dnn-bi-logs/dnn-bi-logs.model';
import { eventLogSeverity, EventLogSeverityInfo } from '../enums/EventLogSeverity';
import { PersonaBarServicesFramework } from './persona-bar-services-framework';

export class EventLogClient {
  private readonly sf: PersonaBarServicesFramework;
  private readonly requestUrl: string;

  constructor() {
    this.sf = new PersonaBarServicesFramework();
    this.requestUrl = this.sf.getServiceRoot() + 'BulkInstallEventLog/';
  }

  public async browse(pageIndex = 0, severity?: EventLogSeverityInfo, eventType?: string): Promise<BrowseResponse> {
    const severityParam = severity ? `&severity=${severity.eventLogSeverityKey}` : '';
    const eventTypeParam = eventType ? `&eventType=${eventType}` : '';
    const response = await fetch(`${this.requestUrl}Browse?pageIndex=${pageIndex}${severityParam}${eventTypeParam}`, {
      headers: this.sf.getModuleHeaders(),
    });
    const responseBody = (await response.json()) as ResponseBody;
    return {
      data: responseBody.Data.map(e => EventLogClient.toEvent(e)),
      pagination: EventLogClient.toPagination(responseBody.Pagination),
    };
  }

  public async getEventTypes(): Promise<string[]> {
    const response = await fetch(`${this.requestUrl}EventTypes`, { headers: this.sf.getModuleHeaders() });
    return (await response.json()) as string[];
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
        throw new Error(`Unknown severity: ${severity as number}`);
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

  private static toPagination(pagination: PaginationResponse): Pagination {
    return {
      currentPage: pagination.CurrentPage,
      pages: pagination.Pages,
    };
  }
}

export interface BrowseResponse {
  data: Event[];
  pagination: Pagination;
}

export interface Pagination {
  pages: number;
  currentPage: number;
}

interface ResponseBody {
  Data: EventLogResponse[];
  Pagination: PaginationResponse;
}

interface PaginationResponse {
  Pages: number;
  CurrentPage: number;
}

interface EventLogResponse {
  EventLogID: number;
  Date: string;
  EventType: string;
  Severity: EventLogSeverityResponse;
  Message: string;
  StackTrace: string;
}

enum EventLogSeverityResponse {
  Info = 0,
  Warning = 1,
  Alert = 2,
  Critical = 3,
}
