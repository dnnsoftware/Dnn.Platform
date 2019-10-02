import React, { Component } from "react";
import { storiesOf } from "@storybook/react";
import ContentLoadWrapper from "./index";
import { TableEmptyState } from "../SvgIcons";

class MyContentLoadWrapper extends Component{
  constructor(){
    super();
    this.state = { loaded: false };    
  }

  componentDidMount(){
    setTimeout(function(){
      this.setState({ loaded: true });
    }.bind(this), 2000);
  }

  render(){
    return (
      <ContentLoadWrapper
        loadComplete={this.state.loaded}
        svgSkeleton={<div dangerouslySetInnerHTML={{ __html: TableEmptyState }} />}
      >
        <div>
          <div className="auditCheckItems">
            <ul>
              <li>Item 1</li>
              <li>Item 2</li>
              <li>Item 3</li>
            </ul>
          </div>
        </div>
      </ContentLoadWrapper>  
    );
  }
}

storiesOf("ContentLoadWrapper", module).add("with loading", () => (
  <ContentLoadWrapper
    loadComplete={false}
    svgSkeleton={<div dangerouslySetInnerHTML={{ __html: TableEmptyState }} />}
  >
    <div>
      <div className="auditcheck-topbar">Loading...</div>
      <div className="auditCheckItems">
        <ul>
          <li>Item 1</li>
          <li>Item 2</li>
          <li>Item 3</li>
        </ul>
      </div>
    </div>
  </ContentLoadWrapper>
));

storiesOf("ContentLoadWrapper", module).add("with content", () => (
  <ContentLoadWrapper
    loadComplete={true}
    svgSkeleton={<div dangerouslySetInnerHTML={{ __html: TableEmptyState }} />}
  >
    <div>
      <div className="auditcheck-topbar">
        <h1>Content</h1>
      </div>

      <div className="auditCheckItems">
        <ul>
          <li>Item 1</li>
          <li>Item 2</li>
          <li>Item 3</li>
        </ul>
      </div>
    </div>
  </ContentLoadWrapper>
));

storiesOf("ContentLoadWrapper", module).add("loaded after 2 seconds", () => (
  <MyContentLoadWrapper />
));