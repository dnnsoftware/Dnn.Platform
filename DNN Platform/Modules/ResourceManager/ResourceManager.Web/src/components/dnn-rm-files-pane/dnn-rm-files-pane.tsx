import { Component, Element, Host, h, Prop, Listen } from "@stencil/core";
import state from "../../store/store";
import { ItemsClient } from "../../services/ItemsClient";
import { Debounce } from "@dnncommunity/dnn-elements";
@Component({
  tag: "dnn-rm-files-pane",
  styleUrl: "dnn-rm-files-pane.scss",
  shadow: true,
})
export class DnnRmFilesPane {
  /** Defines how much more pixels to load under the fold. */
  @Prop() preloadOffset: number = 5000;

  @Element() el: HTMLDnnRmFilesPaneElement;

  private readonly itemsClient: ItemsClient;
  private loadedFilesArea: HTMLDivElement;
  private spacer: HTMLDivElement;

  constructor() {
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  @Listen("scroll")
  handleScroll() {
    this.checkIfMoreItemsNeeded();
  }

  @Listen("dnnRmFoldersChanged", { target: "document" })
  handleFoldersChanged() {
    state.currentItems = {
      ...state.currentItems,
      items: [],
    };
    state.selectedItems = [];
    this.checkIfMoreItemsNeeded();
  }

  @Listen("dnnRmFileDoubleClicked", { target: "document" })
  handleFileDoubleClicked(e: CustomEvent<string>) {
    window.open(e.detail, "_blank");
  }

  componentDidUpdate() {
    const loadedFilesHeight =
      this.loadedFilesArea.getBoundingClientRect().height;
    const heightPerItem = loadedFilesHeight / state.currentItems.items.length;
    const totalNeededHeight = heightPerItem * state.currentItems.totalCount;
    let spacerHeight = totalNeededHeight - loadedFilesHeight;
    if (spacerHeight < 0) {
      spacerHeight = 0;
    }
    this.spacer.style.height = `${spacerHeight}px`;

    this.checkIfMoreItemsNeeded();
  }

  checkIfMoreItemsNeeded() {
    const endOfFilesHeight = this.spacer.getBoundingClientRect().top;
    const thisBottom = this.el.getBoundingClientRect().bottom;
    const offset = endOfFilesHeight - thisBottom;
    if (offset < this.preloadOffset) {
      this.loadMore();
    }
  }

  @Debounce(50)
  loadMore() {
    if (state.currentItems.items.length >= state.currentItems.totalCount) {
      return;
    }

    if (state.itemsSearchTerm) {
      this.itemsClient
        .search(
          state.currentItems.folder.folderId,
          state.itemsSearchTerm,
          state.lastSearchRequestedPage + 1,
          state.pageSize,
          state.sortField,
        )
        .then((data) => {
          state.lastSearchRequestedPage += 1;
          state.currentItems = {
            ...state.currentItems,
            totalCount: data.totalCount,
            items: [...state.currentItems.items, ...data.items],
          };
        });
    } else {
      this.itemsClient
        .getFolderContent(
          state.currentItems.folder.folderId,
          state.currentItems.items.length,
          state.pageSize,
          state.sortField,
        )
        .then(
          (data) =>
            (state.currentItems = {
              ...state.currentItems,
              items: [...state.currentItems.items, ...data.items],
            }),
        )
        .catch(() => {}); // On purpose, we want to ignore aborted requests.
    }
  }

  render() {
    return (
      <Host>
        <div ref={(el) => (this.loadedFilesArea = el)}>
          {state.layout == "list" && (
            <dnn-rm-items-listview
              currentItems={state.currentItems}
            ></dnn-rm-items-listview>
          )}
          {state.layout == "card" && (
            <dnn-rm-items-cardview
              currentItems={state.currentItems}
            ></dnn-rm-items-cardview>
          )}
        </div>
        <div ref={(el) => (this.spacer = el)}></div>
      </Host>
    );
  }
}
