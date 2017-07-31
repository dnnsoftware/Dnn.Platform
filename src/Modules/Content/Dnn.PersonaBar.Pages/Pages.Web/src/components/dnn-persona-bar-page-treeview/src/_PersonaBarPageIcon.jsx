import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import {PagesIcon, FolderIcon, TreeLinkIcon, TreeDraftIcon, TreePaperClip} from "dnn-svg-icons";

export default class PersonaBarPageIcon extends Component {
    /* eslint-disable react/no-danger */
    selectIcon(number) {
        // 1. PagesIcon
        // 2. FolderIcon
        // 3. RightLink
        // 4. LeftLink

         /*eslint-disable react/no-danger*/
        switch(number) {
            case "normal":
               return (<div dangerouslySetInnerHTML={{ __html: PagesIcon }} />);

            case "file":
                return (<div dangerouslySetInnerHTML={{ __html: FolderIcon }} />);

            case "url":
                return ( <div dangerouslySetInnerHTML={{ __html: TreeLinkIcon }} /> );

            default:
               return (<div dangerouslySetInnerHTML={{ __html: PagesIcon }} />);
        }
    }

    render() {
        return (
            <div className="dnn-persona-bar-treeview-icon">
                {this.selectIcon(this.props.iconType)}
            </div>
        );
    }

}

PersonaBarPageIcon.propTypes = {
    iconType: PropTypes.number.isRequired
};