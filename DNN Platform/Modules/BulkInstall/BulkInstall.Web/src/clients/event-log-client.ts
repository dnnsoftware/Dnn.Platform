import { DnnServicesFramework } from '@dnncommunity/dnn-elements';
import type { Event } from '../components/tabs/dnn-bi-logs/dnn-bi-logs.model';
import { eventLogSeverity, EventLogSeverityInfo } from '../enums/EventLogSeverity';

export class EventLogClient {
  private readonly sf: DnnServicesFramework;
  private readonly requestUrl: string;

  constructor(moduleId: number) {
    this.sf = new DnnServicesFramework(moduleId);
    this.requestUrl = this.sf.getServiceRoot('BulkInstall') + 'EventLog/';
  }

  public async browse(pageIndex = 0): Promise<BrowseResponse> {
    const response = await fetch(`${this.requestUrl}Browse?pageIndex=${pageIndex}`, {
      headers: this.sf.getModuleHeaders(),
    });
    const responseBody = (await response.json()) as ResponseBody;
    return {
      data: responseBody.Data.map(e => EventLogClient.toEvent(e)),
      pagination: EventLogClient.toPagination(responseBody.Pagination),
    };
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
      records: pagination.Records,
      navigation: {
        next: pagination.Navigation.Next,
        previous: pagination.Navigation.Previous,
      },
    };
  }
}

export interface BrowseResponse {
  data: Event[];
  pagination: Pagination;
}

export interface Pagination {
  records: number;
  pages: number;
  currentPage: number;
  navigation: {
    previous?: string;
    next?: string;
  };
}

interface ResponseBody {
  Data: EventLogResponse[];
  Pagination: PaginationResponse;
}

interface PaginationResponse {
  Records: number;
  Pages: number;
  CurrentPage: number;
  Navigation: {
    Previous?: string;
    Next?: string;
  };
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
