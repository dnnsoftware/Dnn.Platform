import { Component, h } from '@stencil/core';
@Component({
  tag: 'dnn-bi-circle-x-icon',
  styles: '.icon { height: 48px; width: 48px; }',
})
export class DnnBiCircleXIcon {
  render() {
    return (
      <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 48 48">
        <path
          fill-rule="evenodd"
          d="M 24,44 C 35.04575,44 44,35.04575 44,24 44,12.9543 35.04575,4 24,4 12.9543,4 4,12.9543 4,24 4,35.04575 12.9543,44 24,44 Z M 14.732225,18.267775 20.464475,24 l -5.73225,5.73225 3.53555,3.5355 L 24,27.535525 29.73225,33.26775 33.26775,29.73225 27.535525,24 33.26775,18.267775 29.73225,14.732225 24,20.464475 18.267775,14.732225 Z"
        />
      </svg>
    );
  }
}
