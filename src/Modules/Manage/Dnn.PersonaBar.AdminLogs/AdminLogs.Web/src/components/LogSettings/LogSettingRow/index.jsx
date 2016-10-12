import React, {Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import Collapse from "react-collapse";
import GridCell from "dnn-grid-cell";
import "./style.less";
import { EditIcon } from "dnn-svg-icons";


class LogSettingRow extends Component {
    constructor() {
        super();
        this.handleClick = this.handleClick.bind(this);
    }
    componentDidMount() {
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
    }
    componentWillMount() {
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
    }

    handleClick(event) {
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }

        if (!ReactDOM.findDOMNode(this).contains(event.target) && (typeof event.target.className === "string" && event.target.className.indexOf("do-not-close") === -1)) {

            this.timeout = 475;
            if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
                this.props.Collapse();
            }
        } else {

            this.timeout = 0;
        }
    }
    toggle() {
        if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
            this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.id);
        }
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        let uniqueId = "settingrow-" + Math.random() + Date.now();

        if (props.visible) {
            return (
                <div className={"collapsible-component1"} id={uniqueId}>
                    <div className={"collapsible-header1 " + !opened} >
                        <GridCell title={props.typeName} columnSize={40} >
                            {props.typeName}</GridCell>
                        <GridCell  columnSize={20} >
                            {props.website}</GridCell>
                        <GridCell  columnSize={15} >
                            {props.activeStatus}</GridCell>
                        <GridCell  columnSize={20} >
                            {props.fileName}&nbsp; </GridCell>
                        <GridCell  columnSize={5} >
                            <div className={"edit-icon " + !opened} dangerouslySetInnerHTML={{ __html: EditIcon }} onClick={this.toggle.bind(this) }>
                            </div>
                        </GridCell>
                    </div>
                    <Collapse accordion={true} isOpened={opened} style={{ float: "left" }}>{opened && props.children }</Collapse>
                </div>
            );
        } else {
            return <div/>;
        }
    }
}

LogSettingRow.propTypes = {
    cssClass: PropTypes.string,
    typeName: PropTypes.string,
    website: PropTypes.string,
    activeStatus: PropTypes.string,
    fileName: PropTypes.string,
    className: PropTypes.string,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    visible: PropTypes.bool
};

LogSettingRow.defaultProps = {
    collapsed: true,
    visible: true
};
export default (LogSettingRow);
