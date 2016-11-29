import React, {Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import Collapse from "dnn-collapsible";
import TextOverflowWrapper from "dnn-text-overflow-wrapper";
import styles from "./style.less";
import util from "utils";
import { TrashIcon } from "dnn-svg-icons";

/* eslint-disable quotes */
const SimpleType = require(`!raw!./svg/vocabulary_simple.svg`);
const HierarchyType = require(`!raw!./svg/vocabulary_hierarchy.svg`);


class TermHeader extends Component {
    constructor() {
        super();
        this.state = {
            collapsed: true,
            collapsedClass: true,
            repainting: false
        };
        this.timeout = 0;
        this.handleClick = this.handleClick.bind(this);
    }
    componentDidMount() {
        const {props} = this;
        if (!props.closeOnClick) {
            return;
        }
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
    }
    handleClick(event) {
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }
        if (!ReactDOM.findDOMNode(this).contains(event.target) &&
            (typeof event.target.className === "string" && event.target.className.indexOf("false") > -1)) {

            if (event.target.className.indexOf("delete-button") > -1) {
                return;
            }
            this.timeout = 475;
            this.collapse();
        } else {

            this.timeout = 0;
        }
    }
    uncollapse() {
        this.setState({
            collapsed: false
        });
    }
    collapse() {
        this.setState({
            collapsed: true
        });
    }
    toggle(event) {
        if (this.state.collapsed) {
            this.uncollapse();
        } else {
            if (event.target.parentNode.className.indexOf("delete-button") > -1) {
                return;
            }
            this.collapse();
        }
    }
    onDelete() {
        const {props} = this;
        props.onDelete(props.term, props.index, () => { this.collapse(); });
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        const svgIcon = props.type === 1 ? SimpleType : HierarchyType;
        return (
            <div className={"" + styles.collapsibleComponent + (props.className ? (" " + props.className) : "") }>
                <div className={"collapsible-header " + state.collapsed} onClick={this.toggle.bind(this) }>
                    <div className="term-header">
                        <div className="term-icon" dangerouslySetInnerHTML={{ __html: svgIcon }}>
                        </div>
                        <div className="term-label">

                            <TextOverflowWrapper text={props.header} maxWidth={200}/>

                            {(!this.state.collapsed && !props.term.IsSystem) && util.settings.isHost &&
                                <div dangerouslySetInnerHTML={{ __html: TrashIcon }} onClick={this.onDelete.bind(this) } className="delete-button"></div>
                            }
                        </div>
                    </div>
                    {!props.disabled &&
                        <span
                            className={"collapse-icon " + (this.state.collapsed ? "collapsed" : "") }>
                        </span>
                    }
                </div>
                <Collapse isOpened={!this.state.collapsed} style={{ float: "left" }}
                    fixedHeight={550}>{!state.collapsed && props.children }</Collapse>
            </div >
        );
    }
}

TermHeader.propTypes = {
    header: PropTypes.string,
    type: PropTypes.string,
    label: PropTypes.node,
    children: PropTypes.node,
    disabled: PropTypes.bool,
    className: PropTypes.string,
    closeOnClick: PropTypes.bool,
    term: PropTypes.object,
    onDelete: PropTypes.func,
    index: PropTypes.number
};
export default TermHeader;