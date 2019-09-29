import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";

export default class PersonaBarPageIcon extends Component {
    /* eslint-disable react/no-danger */
    selectIcon(number) {

         /*eslint-disable react/no-danger*/
        switch(number) {
            case "normal":
                return (<div dangerouslySetInnerHTML={{ __html: SvgIcons.PagesIcon }} />);

            case "file":
                return (<div dangerouslySetInnerHTML={{ __html: SvgIcons.TreePaperClip }} />);

            case "tab":
            case "url":
                return ( <div dangerouslySetInnerHTML={{ __html: SvgIcons.TreeLinkIcon }} /> );

            case "existing":
                return ( <div dangerouslySetInnerHTML={{ __html: SvgIcons.TreeLinkIcon }} /> );

            default:
                return (<div dangerouslySetInnerHTML={{ __html: SvgIcons.PagesIcon }}/>);
        }
    }

    render() {
        return (
            <div  className={(this.props.selected ) ? "dnn-persona-bar-treeview-icon selected-item " : "dnn-persona-bar-treeview-icon"}>
                {this.selectIcon(this.props.iconType)}
            </div>
        );
    }

}

PersonaBarPageIcon.propTypes = {
    iconType: PropTypes.string.isRequired,
    selected: PropTypes.bool.isRequired
};