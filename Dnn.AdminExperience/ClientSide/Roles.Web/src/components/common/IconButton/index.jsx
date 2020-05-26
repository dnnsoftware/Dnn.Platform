import React, {Component } from "react";
import PropTypes from "prop-types";
import { SvgIcons }  from "@dnnsoftware/dnn-react-common";


import "./style.less";

class IconButton extends Component {
    constructor(props) {
        super(props);
    }

    getIcon() {
        const {props} = this;

        switch (props.type.toLowerCase()) {
            case "add":
                return SvgIcons.AddIcon;
            case "edit":
                return SvgIcons.EditIcon;
            case "card":
                return SvgIcons.CardViewIcon;
            case "list":
                return SvgIcons.ListViewIcon;
            case "preview":
                return SvgIcons.PreviewIcon;
            case "settings":
                return SvgIcons.SettingsIcon;
            case "page":
                return SvgIcons.PageIcon;
            case "traffic":
                return SvgIcons.TrafficIcon;
            case "template":
                return SvgIcons.TemplateIcon;
            case "trash":
                return SvgIcons.TrashIcon;
            case "user":
                return SvgIcons.UserIcon;
            case "arrow-down":
                return SvgIcons.ArrowDownIcon;
            case "arrow-right":
                return SvgIcons.ArrowRightIcon;
            case "arrow-up":
                return SvgIcons.ArrowUpIcon;
            case "lock-closed":
                return SvgIcons.LockClosedIcon;
            default:
                return require("!raw-loader!../../../img/common/" + props.type.toLowerCase() + ".svg").default;
        }
    }

    getClassName() {
        const {props} = this;

        let name = "icon-button";
        if (props.className) {
            name += " " + props.className;
        }

        return name;
    }

    getStyle() {
        const {props} = this;

        let style = {};
        if (props.width && props.width > 0) {
            style["width"] = props.width + "px";
        }
        if (props.height && props.height > 0) {
            style["height"] = props.height + "px";
        }

        return style;
    }

    onClick(event) {
        const {props} = this;

        event.preventDefault();

        props.onClick(event);
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;

        if (typeof props.onClick === "function") {
            return <a href="#" className={this.getClassName() } style={this.getStyle() } dangerouslySetInnerHTML={{ __html: this.getIcon() }} title={props.title} onClick={this.onClick.bind(this) } aria-label={props.type} />;
        } else {
            return <span className="icon-flat"  style={this.getStyle() } dangerouslySetInnerHTML={{ __html: this.getIcon() }} title={props.title}/>;
        }
    }
}

IconButton.propTypes = {
    type: PropTypes.string,
    onClick: PropTypes.func,
    width: PropTypes.number,
    height: PropTypes.number,
    title: PropTypes.string
};

export default IconButton;