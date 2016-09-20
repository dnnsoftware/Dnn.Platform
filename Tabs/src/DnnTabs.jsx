import React, {Component, PropTypes } from "react";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import "./style.less";

class DnnTabs extends Component {

    constructor() {
        super();
        this.uniqueId = Date.now() * Math.random();
        this.state = {
            selectedIndex: 0
        };
    }

    onSelect(index, last) {
        const { props } = this;
        if (typeof props.selectedIndex === "undefined") {
            this.setState({
                selectedIndex: index
            });
        }
        if (props.onSelect) {
            props.onSelect(index, last);
        }
    }

    getTabHeaders() {
        const {props} = this;
        return props.tabHeaders.map((tabHeader, index) => {
            return <Tab key={this.uniqueId + "-" + index}>{tabHeader}</Tab>;
        });
    }
    getTabPanels() {
        const {props} = this;
        let children = [];
        for (let i = 0; i < (props.children.length); i++) {
            children.push(<TabPanel key={this.uniqueId + "panel-" + i}>{props.children[i]}</TabPanel>);
        }
        return children;
    }

    render() {
        const {props, state} = this;
        const className = (props.className ? " " + props.className : "");
        return (
            <Tabs className={"dnn-tabs" + className + (" " + props.type) } onSelect={this.onSelect.bind(this) }
                selectedIndex={typeof props.selectedIndex !== "undefined" ? props.selectedIndex : state.selectedIndex}>
                <TabList>
                    {this.getTabHeaders() }
                </TabList>
                {this.getTabPanels() }
            </Tabs>
        );
    }
}

DnnTabs.propTypes = {
    selectedIndex: PropTypes.number,
    tabHeaders: PropTypes.array,
    className: PropTypes.string,
    children: PropTypes.node,
    onSelect: PropTypes.func
};

DnnTabs.defaultProps = {
    type: "primary"
};

export default DnnTabs;