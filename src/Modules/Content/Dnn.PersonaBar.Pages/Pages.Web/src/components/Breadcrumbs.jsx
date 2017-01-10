import React, {PropTypes} from "react";

const maxItems = 4;

const Breadcrumbs = ({items, onSelectedItem}) => {
    return (
        <div className="breadcrumbs-container">
            { items.length > maxItems && 
                <span className="more" 
                    title={items.map(i => i.name).join(" > ")}
                    onClick={onSelectedItem.bind(this, items[0])} /> }
            { items.slice(Math.max(items.length - maxItems, 0)).map(item =>
                <div key={item.id} onClick={onSelectedItem.bind(this, item)}>
                    <span>{item.name}</span>
                </div>)
            }
        </div>
    );
};

Breadcrumbs.propTypes = {
    items: PropTypes.array.isRequired,
    onSelectedItem: PropTypes.func.isRequired
};

export default Breadcrumbs;