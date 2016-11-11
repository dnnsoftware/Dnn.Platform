import React, {Component, PropTypes} from "react";
import { AddIcon, LinkIcon } from "dnn-svg-icons";
import Localization from "../../../localization";
import EditUrl from "./EditUrl";
import styles from "./style.less";

class NewUrl extends Component {
    render() {
        const {url, pageHasParent, addingNewUrl, opened, siteAliases, primaryAliasId, onAdd, onChange, onOpenNewForm, onCancel} = this.props;
        
        return (
            /* eslint-disable react/no-danger */
            <div className={styles.AddItemRow}>
                <div className="link-icon" dangerouslySetInnerHTML={{ __html: LinkIcon }} />
                <div className="sectionTitle">{Localization.get("UrlsForThisPage")}</div>
                {!opened &&
                <div className="AddItemBox" onClick={onOpenNewForm}>
                    <div className="add-icon" dangerouslySetInnerHTML={{ __html: AddIcon }}>
                    </div> {Localization.get("AddUrl")}
                </div>}
                {opened &&
                <EditUrl url={url} 
                    saving={addingNewUrl}
                    pageHasParent={pageHasParent}
                    accordion={true} isOpened={opened} keepCollapsedContent={true}
                    onChange={onChange}
                    onSave={onAdd} 
                    onCancel={onCancel} 
                    siteAliases={siteAliases}
                    primaryAliasId={primaryAliasId}
                    className="newUrlContainer" />}
            </div>
            /* eslint-enable react/no-danger */
        );
    }
}

NewUrl.propTypes = {
    url: PropTypes.object.isRequired,
    opened: PropTypes.bool,
    pageHasParent: PropTypes.bool,
    onAdd: PropTypes.func.isRequired,
    onChange: PropTypes.func.isRequired,
    onOpenNewForm: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
    addingNewUrl: PropTypes.bool,
    siteAliases: PropTypes.arrayOf(PropTypes.object).isRequired,
    primaryAliasId: PropTypes.number
};

export default NewUrl;