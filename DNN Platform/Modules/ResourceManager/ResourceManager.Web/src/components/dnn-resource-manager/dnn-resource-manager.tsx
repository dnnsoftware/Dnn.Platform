import { Component, Host, h, State, Watch, Prop } from '@stencil/core';
import state from "../../store/store";
import { LocalizationClient } from "../../services/LocalizationClient";
import { sortField } from '../../enums/SortField';

const localStorageSplitWidthKey = "dnn-resource-manager-last-folders-width";
@Component({
  tag: 'dnn-resource-manager',
  styleUrl: 'dnn-resource-manager.scss',
  shadow: true,
})
export class DnnResourceManager {

  @Prop() moduleId!: number;

  constructor() {
    state.moduleId = this.moduleId;
    this.localizationClient = new LocalizationClient(this.moduleId);
  }

  @State() foldersExpanded = true;

  @Watch("foldersExpanded") async foldersExpandedChanged(expanded: boolean){
    const lastWidth = parseFloat(localStorage.getItem(localStorageSplitWidthKey)) || 30;
    if (expanded){
      this.splitView.setSplitWidthPercentage(lastWidth);
      return;
    }

    this.splitView.setSplitWidthPercentage(0);
  }

  componentWillLoad() {
    return new Promise<void>((resolve, reject) => {
      this.localizationClient.getResources()
      .then(resources =>
      {
        state.localization = resources;
        state.sortField = sortField.itemName; 
        resolve();
      })
      .catch(error => {
        console.error(error);
        reject();
      });
    })
  }
  
  private splitView: HTMLDnnVerticalSplitviewElement;
  private localizationClient: LocalizationClient;

  private handleSplitWidthChanged(event: CustomEvent<number>){
    if (event.detail != 0){
      localStorage.setItem(localStorageSplitWidthKey, event.detail.toString());
    }
  }

  render() {
    return (
      <Host>
        <div class="container">
          <dnn-rm-top-bar></dnn-rm-top-bar>
          <dnn-vertical-splitview
            ref={el => this.splitView = el}
            onWidthChanged={this.handleSplitWidthChanged}
          >
            <div class="splitter">
              <button
                class={this.foldersExpanded && "expanded"}
                onClick={() => this.foldersExpanded = !this.foldersExpanded}
              >
                <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0V0z" fill="none"/><path d="M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6-6-6z"/></svg>
              </button>
            </div>
            <dnn-rm-left-pane slot="left"></dnn-rm-left-pane>
            <dnn-rm-right-pane slot="right"></dnn-rm-right-pane>
          </dnn-vertical-splitview>
        </div>
      </Host>
    );
  }
}
