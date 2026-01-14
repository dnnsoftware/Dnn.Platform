import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, SvgIcons } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import UrlRow from "./UrlRow";
import EditUrl from "./EditUrl";
import styles from "./style.module.less";
import uniqBy from "lodash/uniqBy";

class Table extends Component {
    generateUrlKey(url) {
        return `url_${url.id}_${url.path}_${url.statusCode.Key}`;
    }

    getUrlRows(pageUrls) {
        if (!pageUrls || pageUrls.length === 0) {
            return null;
        }

        const deduplicatedUrls = uniqBy(pageUrls, url => this.generateUrlKey(url));

        const { siteAliases, primaryAliasId, onSave, onCancel, onDelete, onChange, editedUrl,
            pageHasParent, editingUrl, onOpenEditForm } = this.props;
        return deduplicatedUrls.map((url) => {
            return <UrlRow 
                key={this.generateUrlKey(url)}
                url={url}
                editedUrl={editedUrl}
                onOpenEditForm={onOpenEditForm}
                pageHasParent={pageHasParent}
                siteAliases={siteAliases}
                primaryAliasId={primaryAliasId}
                onChange={onChange}
                onSave={onSave}
                onCancel={onCancel}
                saving={editingUrl}
                onDelete={onDelete} />;
        });
    }

    onAddNewUrl() {
        this.props.onAddNewUrl(this.props.editedUrl, this.props.primaryAliasId);
    }

    render() {
        const { pageUrls, pageHasParent, addingNewUrl, onOpenNewForm, onCloseNewUrl,
            newFormOpened, onChange, editedUrl, siteAliases, primaryAliasId } = this.props;
        const urlRows = this.getUrlRows(pageUrls);

         
        return (
            <div>
                <div className={styles.addItemRow}>
                    <div className="link-icon"><SvgIcons.LinkIcon /></div>
                    <div className="sectionTitle">{Localization.get("UrlsForThisPage")}</div>
                    <div className={"AddItemBox" + (newFormOpened ? " active" : "")} onClick={onOpenNewForm}>
                        <div className={"add-icon" + (newFormOpened ? " active" : "")}><SvgIcons.AddIcon />
                        </div> {Localization.get("AddUrl")}
                    </div>
                </div>
                <div className="url-table">
                    <div className="header-row">
                        <GridCell columnSize={50} >
                            {Localization.get("Url")}
                        </GridCell>
                        <GridCell columnSize={20} >
                            {Localization.get("UrlType")}
                        </GridCell>
                        <GridCell columnSize={30} >
                            {Localization.get("GeneratedBy")}
                        </GridCell>
                    </div>
                    {newFormOpened &&
                        <div className={styles.urlRow + (newFormOpened ? " row-opened" : "")}>
                            <GridCell columnSize={50} >
                                {"-"}
                            </GridCell>
                            <GridCell columnSize={20} >
                                {"-"}
                            </GridCell>
                            <GridCell columnSize={20} >
                                {"-"}
                            </GridCell>
                            <EditUrl url={editedUrl}
                                saving={addingNewUrl}
                                pageHasParent={pageHasParent}
                                accordion={true} isOpened={newFormOpened}
                                onChange={onChange}
                                onSave={this.onAddNewUrl.bind(this)}
                                onCancel={onCloseNewUrl}
                                siteAliases={siteAliases}
                                primaryAliasId={primaryAliasId}
                                className="newUrlContainer" />
                        </div>}
                    {urlRows}
                </div>
            </div>
        );
    }
}

Table.propTypes = {
    pageUrls: PropTypes.arrayOf(PropTypes.object),
    siteAliases: PropTypes.arrayOf(PropTypes.object).isRequired,
    primaryAliasId: PropTypes.number,
    onSave: PropTypes.func.isRequired,
    onChange: PropTypes.func,
    onDelete: PropTypes.func,
    pageHasParent: PropTypes.bool,
    onOpenEditForm: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
    editingUrl: PropTypes.bool,
    editedUrl: PropTypes.object,
    addingNewUrl: PropTypes.bool,
    onOpenNewForm: PropTypes.func.isRequired,
    onCloseNewUrl: PropTypes.func.isRequired,
    newFormOpened: PropTypes.bool,
    onAddNewUrl: PropTypes.func.isRequired
};

export default Table;