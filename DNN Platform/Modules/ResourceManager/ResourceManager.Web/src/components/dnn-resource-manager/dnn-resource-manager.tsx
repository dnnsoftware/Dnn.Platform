import { Component, Element, Host, h, State, Watch, Prop } from "@stencil/core";
import state from "../../store/store";
import { LocalizationClient } from "../../services/LocalizationClient";
import { sortField } from "../../enums/SortField";
import { ItemsClient } from "../../services/ItemsClient";

const localStorageSplitWidthKey = "dnn-resource-manager-last-folders-width";
@Component({
  tag: "dnn-resource-manager",
  styleUrl: "dnn-resource-manager.scss",
  shadow: true,
})
export class DnnResourceManager {
  /** The ID of the module. */
  @Prop() moduleId!: number;

  private splitView: HTMLDnnVerticalSplitviewElement;
  private localizationClient: LocalizationClient;
  private folderMappingsModal: HTMLDnnModalElement;
  private itemsClient: ItemsClient;

  constructor() {
    state.moduleId = this.moduleId;
    this.localizationClient = new LocalizationClient(this.moduleId);
    this.itemsClient = new ItemsClient(this.moduleId);
  }

  @Element() el: HTMLDnnResourceManagerElement;

  @State() foldersExpanded = true;

  @Watch("foldersExpanded") async foldersExpandedChanged(expanded: boolean) {
    const lastWidth =
      parseFloat(localStorage.getItem(localStorageSplitWidthKey)) || 30;
    if (expanded) {
      this.splitView.setSplitWidthPercentage(lastWidth);
      return;
    }

    this.splitView.setSplitWidthPercentage(0);
  }

  componentWillLoad() {
    const getResourcesPromise = this.localizationClient
      .getResources()
      .then((resources) => {
        state.localization = resources;
        state.sortField = sortField.itemName;
      })
      .catch((error) => {
        console.error(error);
      });

    const getSettingsPromise = this.itemsClient
      .getSettings()
      .then((settings) => {
        state.settings = settings;
      })
      .catch((error) => {
        console.error(error);
      });

    const canManageFolderTypes = this.itemsClient
      .canManageFolderTypes()
      .then((canManage) => {
        state.canManageFolderTypes = canManage;
      })
      .catch((error) => {
        console.error(error);
      });

    return Promise.all([
      getResourcesPromise,
      getSettingsPromise,
      canManageFolderTypes,
    ]);
  }

  private handleSplitWidthChanged(event: CustomEvent<number>) {
    if (event.detail != 0) {
      localStorage.setItem(localStorageSplitWidthKey, event.detail.toString());
    }
  }

  render() {
    return (
      <Host>
        <div class="container">
          <dnn-rm-top-bar></dnn-rm-top-bar>
          <dnn-vertical-splitview
            ref={(el) => (this.splitView = el)}
            onWidthChanged={this.handleSplitWidthChanged}
          >
            <div class="splitter">
              <button
                type="button"
                class={this.foldersExpanded && "expanded"}
                onClick={() => (this.foldersExpanded = !this.foldersExpanded)}
                title={
                  this.foldersExpanded
                    ? state?.localization?.CollapseFolders
                    : state?.localization?.ExpandFolders
                }
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  height="24px"
                  viewBox="0 0 24 24"
                  width="24px"
                  fill="#000000"
                >
                  <path d="M0 0h24v24H0V0z" fill="none" />
                  <path d="M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6-6-6z" />
                </svg>
              </button>
            </div>
            <dnn-rm-left-pane slot="left"></dnn-rm-left-pane>
            {state.canManageFolderTypes && (
              <button
                slot="left"
                class="folder-mappings"
                title={state.localization?.EditFolderMappings}
                onClick={() => this.folderMappingsModal.show()}
              >
                <svg xmlns="http://www.w3.org/2000/svg" height="24" width="24">
                  <path d="m9.25 22-.4-3.2q-.325-.125-.612-.3-.288-.175-.563-.375L4.7 19.375l-2.75-4.75 2.575-1.95Q4.5 12.5 4.5 12.337v-.675q0-.162.025-.337L1.95 9.375l2.75-4.75 2.975 1.25q.275-.2.575-.375.3-.175.6-.3l.4-3.2h5.5l.4 3.2q.325.125.613.3.287.175.562.375l2.975-1.25 2.75 4.75-2.575 1.95q.025.175.025.337v.675q0 .163-.05.338l2.575 1.95-2.75 4.75-2.95-1.25q-.275.2-.575.375-.3.175-.6.3l-.4 3.2Zm2.8-6.5q1.45 0 2.475-1.025Q15.55 13.45 15.55 12q0-1.45-1.025-2.475Q13.5 8.5 12.05 8.5q-1.475 0-2.488 1.025Q8.55 10.55 8.55 12q0 1.45 1.012 2.475Q10.575 15.5 12.05 15.5Zm0-2q-.625 0-1.062-.438-.438-.437-.438-1.062t.438-1.062q.437-.438 1.062-.438t1.063.438q.437.437.437 1.062t-.437 1.062q-.438.438-1.063.438ZM12 12Zm-1 8h1.975l.35-2.65q.775-.2 1.438-.588.662-.387 1.212-.937l2.475 1.025.975-1.7-2.15-1.625q.125-.35.175-.738.05-.387.05-.787t-.05-.788q-.05-.387-.175-.737l2.15-1.625-.975-1.7-2.475 1.05q-.55-.575-1.212-.963-.663-.387-1.438-.587L13 4h-1.975l-.35 2.65q-.775.2-1.437.587-.663.388-1.213.938L5.55 7.15l-.975 1.7 2.15 1.6q-.125.375-.175.75-.05.375-.05.8 0 .4.05.775t.175.75l-2.15 1.625.975 1.7 2.475-1.05q.55.575 1.213.962.662.388 1.437.588Z" />
                </svg>
              </button>
            )}
            <dnn-rm-right-pane slot="right"></dnn-rm-right-pane>
          </dnn-vertical-splitview>
          {state.canManageFolderTypes && (
            <dnn-modal ref={(el) => (this.folderMappingsModal = el)}>
              <dnn-rm-folder-mappings />
            </dnn-modal>
          )}
        </div>
      </Host>
    );
  }
}
