import { Component, h } from '@stencil/core';
@Component({
  tag: 'bulk-install-clock-icon',
})
export class BulkInstallClockIcon {
  render() {
    return (
      <svg xmlns="http://www.w3.org/2000/svg" height="48" width="48">
        <path d="M 40,24 C 40,15.178 32.822,8 24,8 15.178,8 8,15.178 8,24 c 0,8.822 7.178,16 16,16 8.822,0 16,-7.178 16,-16 m 4,0 C 44,35.046 35.046,44 24,44 12.954,44 4,35.046 4,24 4,12.954 12.954,4 24,4 35.046,4 44,12.954 44,24 m -8,2 c 0,1.104 -0.896,2 -2,2 h -8 c -2.21,0 -4,-1.79 -4,-4 V 14 c 0,-1.104 0.896,-2 2,-2 1.104,0 2,0.896 2,2 v 8 c 0,1.1 0.9,2 2,2 h 6 c 1.104,0 2,0.896 2,2" />
      </svg>
    );
  }
}
