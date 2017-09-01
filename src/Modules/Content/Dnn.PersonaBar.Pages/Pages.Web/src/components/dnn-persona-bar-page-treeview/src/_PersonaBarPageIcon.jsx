import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import {PagesIcon, FolderIcon, TreeLinkIcon, TreeDraftIcon, TreePaperClip} from "dnn-svg-icons";

export default class PersonaBarPageIcon extends Component {
    /* eslint-disable react/no-danger */
    selectIcon(number) {

         /*eslint-disable react/no-danger*/
        switch(number) {
            case "normal":
                return (<div dangerouslySetInnerHTML={{ __html: PagesIcon }} />);

            case "file":
                return (<div dangerouslySetInnerHTML={{ __html: FolderIcon }} />);

            case "tab":
            case "url":
                return ( <div dangerouslySetInnerHTML={{ __html: TreeLinkIcon }} /> );

            case "existing":
                return ( <div dangerouslySetInnerHTML={{ __html: TreeLinkIcon }} /> );

            default:
                return (<div dangerouslySetInnerHTML={{ __html: PagesIcon }}/>);
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
    iconType: PropTypes.number.isRequired,
    selected: PropTypes.bool.isRequired
};