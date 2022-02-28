import { Component, Element, Host, h, Prop, Listen } from '@stencil/core';
import state from '../../store/store';
import { ItemsClient } from "../../services/ItemsClient";
@Component({
  tag: 'dnn-rm-files-pane',
  styleUrl: 'dnn-rm-files-pane.scss',
  shadow: true,
})
export class DnnRmFilesPane {

  @Prop() preloadOffset: number = 1000;
  
  @Element() el: HTMLDnnRmFilesPaneElement;

  private readonly itemsClient: ItemsClient;
  private loadedFilesArea: HTMLDivElement;
  private spacer: HTMLDivElement;

  constructor(){
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  @Listen("scroll")
  handleScroll(){
    this.checkIfMoreItemsNeeded();
  }

  componentDidUpdate() {
    const loadedFilesHeight = this.loadedFilesArea.getBoundingClientRect().height;
    const heightPerItem = loadedFilesHeight / state.currentItems.items.length;
    const totalNeededHeight = heightPerItem * state.currentItems.totalCount;
    let spacerHeight = totalNeededHeight - loadedFilesHeight;
    if (spacerHeight < 0){
      spacerHeight = 0;
    }
    this.spacer.style.height = `${spacerHeight}px`;

    this.checkIfMoreItemsNeeded();
  }

  checkIfMoreItemsNeeded() {
    const endOfFilesHeight = this.spacer.getBoundingClientRect().top;
    const thisBottom = this.el.getBoundingClientRect().bottom;
    const offset = endOfFilesHeight - thisBottom;
    if (offset < this.preloadOffset){
      this.loadMore();
    }
  }

  loadMore() {
    if (state.currentItems.items.length >= state.currentItems.totalCount){
      return;
    }

    this.itemsClient.getFolderContent(
      state.currentItems.folder.folderId,
      state.currentItems.items.length)
    .then(data => state.currentItems = {
      ...state.currentItems,
      items: [...state.currentItems.items, ...data.items],
    });
    console.log(state.currentItems);
  }

  render() {
    return (
      <Host>
        <div ref={el => this.loadedFilesArea = el}>
          {state.layout == "list" &&
            <dnn-rm-items-listview currentItems={state.currentItems}></dnn-rm-items-listview>
          }
          {state.layout == "card" &&
            <dnn-rm-items-cardview currentItems={state.currentItems}></dnn-rm-items-cardview>
          }
        </div>
        <div ref={el => this.spacer = el}></div>
      </Host>
    );
  }

}
