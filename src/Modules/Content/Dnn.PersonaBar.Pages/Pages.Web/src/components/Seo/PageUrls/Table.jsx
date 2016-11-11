import React, {Component, PropTypes} from "react";
import GridCell from "dnn-grid-cell";
import Localization from "../../../localization";
import UrlRow from "./UrlRow";

class Table extends Component {
    
    getUrlRows(pageUrls) {
        if (!pageUrls || pageUrls.length === 0) {
            return null;
        }
        
        const {siteAliases, primaryAliasId, onSave, onCancel, onDelete, onChange, editedUrl,
                pageHasParent, editingUrl, onOpenEditForm} = this.props;
        return pageUrls.map(url => {
            return <UrlRow url={url}
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
    
    render() {
        const {pageUrls} = this.props;
        const urlRows = this.getUrlRows(pageUrls);
        
        return (
            <div className="url-table">
                <div className="header-row">
                    <GridCell columnSize={50} >
                        {Localization.get("Url")}
                    </GridCell>
                    <GridCell  columnSize={20} >
                        {Localization.get("UrlType")}
                    </GridCell>
                    <GridCell  columnSize={30} >
                        {Localization.get("GeneratedBy")}
                    </GridCell>
                </div>
                {urlRows}
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
    editedUrl: PropTypes.object
};

export default Table;