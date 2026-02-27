import { Component, Host, h, Prop, Event, EventEmitter, Fragment } from '@stencil/core';
import { Pagination } from '../../../../clients/event-log-client';

type PageItem = { type: 'page'; index: number; isCurrent: boolean } | { type: 'ellipsis' };

const ellipsisItem: PageItem = { type: 'ellipsis' };
function toPageItem(index: number, current: number): PageItem {
  return {
    type: 'page',
    index: index,
    isCurrent: index === current,
  };
}

function* makePagesIterator(current: number, lastIndex: number) {
  const start = Math.max(current - 5, 0);
  const end = Math.min(start + 10, lastIndex);
  if (start !== 0) {
    yield toPageItem(0, current);
    if (start === 2) {
      yield toPageItem(1, current);
    } else if (start !== 1) {
      yield ellipsisItem;
    }
  }

  for (let i = start; i <= end; i++) {
    yield toPageItem(i, current);
  }

  if (end !== lastIndex) {
    if (end === lastIndex - 2) {
      yield toPageItem(lastIndex - 1, current);
    } else if (end !== lastIndex - 1) {
      yield ellipsisItem;
    }

    yield toPageItem(lastIndex, current);
  }
}

@Component({
  tag: 'dnn-bi-log-pagination',
  styleUrl: 'dnn-bi-log-pagination.scss',
  shadow: true,
})
export class DnnBiLogPagination {
  /** The pagination */
  @Prop() public pagination!: Pagination;

  @Event() public pageSelected: EventEmitter<number>;

  private getPages(): PageItem[] {
    return Array.from(makePagesIterator(this.pagination.currentPage, this.pagination.pages - 1));
  }

  render() {
    return (
      <Host>
        {this.pagination.pages > 1 && (
          <ol class="pagination">
            {this.getPages().map(item => (
              <>
                {item.type === 'page' && (
                  <li class={item.isCurrent ? 'active' : ''}>
                    <button
                      disabled={item.isCurrent}
                      onClick={e => {
                        e.preventDefault();
                        this.pageSelected.emit(item.index);
                      }}
                    >
                      {item.index + 1}
                    </button>
                  </li>
                )}
                {item.type === 'ellipsis' && <li class="ellipsis">…</li>}
              </>
            ))}
          </ol>
        )}
      </Host>
    );
  }
}
