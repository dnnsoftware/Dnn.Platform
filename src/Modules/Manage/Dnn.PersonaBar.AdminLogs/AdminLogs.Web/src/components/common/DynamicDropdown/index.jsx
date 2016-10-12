import React, {PropTypes, Component} from "react";
import Collapse from "react-collapse";
import Scrollbars from "react-custom-scrollbars";
import styles from "./style.less";

const svgIcon = require(`!raw!./arrow_down.svg`);

class DynamicDropdown extends Component {
    constructor() {
        super();
        this.state = {
            dropDownOpen: false
        };
    }
    toggleDropdown() {
        this.setState({
            dropDownOpen: !this.state.dropDownOpen
        });
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div className={styles.dynamicDropdown} onClick={this.toggleDropdown.bind(this) }>
                <div className="dropdown-label" title={props.label}>
                    <a>{props.label}</a>
                </div>
                <div className="dropdown-icon" dangerouslySetInnerHTML={{ __html: svgIcon }}></div>
                <div className="collapsible-content">
                    <Collapse
                        fixedHeight={props.fixedHeight}
                        keepCollapsedContent={props.keepCollapsedContent}
                        isOpened={state.dropDownOpen}>
                        {props.fixedHeight &&
                            <Scrollbars width={props.collapsibleWidth || "100%"} height={props.collapsibleHeight || "100%"} style={props.scrollAreaStyle}>
                                <div>
                                    {props.children}
                                </div>
                            </Scrollbars>
                        }
                        {!props.fixedHeight && props.children}
                    </Collapse>
                </div>
            </div>
        );
    }
}

DynamicDropdown.PropTypes = {
    label: PropTypes.string,
    fixedHeight: PropTypes.number,
    collapsibleWidth: PropTypes.number,
    collapsibleHeight: PropTypes.number,
    keepCollapsedContent: PropTypes.bool,
    scrollAreaStyle: PropTypes.object,
    children: PropTypes.node
};

export default DynamicDropdown;