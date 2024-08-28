import { Component, Host, h, State } from '@stencil/core';
import { IStylesResx } from '../../window.dnn';
import StylesClient from '../../clients/styles-client';

@Component({
  tag: 'dnn-styles-module',
  styleUrl: 'dnn-styles-module.css',
  shadow: true,
})
export class DnnStylesModule {
  @State() resx: IStylesResx;

  private stylesClient: StylesClient;

  constructor() {
    this.stylesClient = new StylesClient();
  }

  componentWillLoad() {
    this.resx = window.dnn.initStyles().utility.resx.Styles;
    document.querySelector("#dnnStylesHeader h3")
      .textContent = this.resx.nav_Styles;
    this.stylesClient.getStyles()
      .then(response => console.log(response))
      .catch(error => console.error(error));
  }

  render() {
    return (
      <Host>
        <p>Styles from stencil...</p>
      </Host>
    );
  }

}
