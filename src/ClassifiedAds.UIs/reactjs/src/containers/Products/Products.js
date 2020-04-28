import React, { Component } from "react";
import { NavLink } from "react-router-dom";
import { connect } from "react-redux";
import { Modal, Button } from "react-bootstrap";

import logo from "../../logo.svg";
import * as actions from "./actions";
import Star from "../../components/Star/Star";

class Products extends Component {
  state = {
    pageTitle: "Product List",
    showImage: false,
    showDeleteModal: false,
    deletingProduct: null,
    listFilter: ""
  };

  toggleImage = () => {
    this.setState({ showImage: !this.state.showImage });
  };

  filterChanged = event => {
    this.setState({ listFilter: event.target.value });
  };

  performFilter(filterBy) {
    filterBy = filterBy.toLocaleLowerCase();
    return this.props.products.filter(
      product => product.name.toLocaleLowerCase().indexOf(filterBy) !== -1
    );
  }

  onRatingClicked = event => {
    const pageTitle = "Product List: " + event;
    this.setState({ pageTitle: pageTitle });
  };

  deleteProduct = product => {
    this.setState({ showDeleteModal: true, deletingProduct: product });
  };

  deleteCanceled = () => {
    this.setState({ showDeleteModal: false, deletingProduct: null });
  };

  deleteConfirmed = () => {
    this.props.deleteProduct(this.state.deletingProduct);
    this.setState({ showDeleteModal: false, deletingProduct: null });
  };

  componentDidMount() {
    this.props.fetchProducts();
  }

  render() {
    const filteredProducts = this.state.listFilter
      ? this.performFilter(this.state.listFilter)
      : this.props.products;

    const rows = filteredProducts?.map(product => (
      <tr key={product.id}>
        <td>
          {this.state.showImage ? (
            <img
              src={product.imageUrl || logo}
              title={product.name}
              style={{ width: "50px", margin: "2px" }}
            />
          ) : null}
        </td>
        <td>
          <NavLink to={"/products/" + product.id}>{product.name}</NavLink>
        </td>
        <td>{product.code?.toLocaleUpperCase()}</td>
        <td>{product.description}</td>
        <td>{product.price || (5).toFixed(2)}</td>
        <td>
          <Star
            rating={product.starRating || 4}
            ratingClicked={event => this.onRatingClicked(event)}
          ></Star>
        </td>
        <td>
          <NavLink
            className="btn btn-primary"
            to={"/products/edit/" + product.id}
          >
            Edit
          </NavLink>
          &nbsp;
          <button
            type="button"
            className="btn btn-primary btn-danger"
            onClick={() => this.deleteProduct(product)}
          >
            Delete
          </button>
        </td>
      </tr>
    ));

    const table = this.props.products ? (
      <table className="table">
        <thead>
          <tr>
            <th>
              <button className="btn btn-primary" onClick={this.toggleImage}>
                {this.state.showImage ? "Hide" : "Show"} Image
              </button>
            </th>
            <th>Product</th>
            <th>Code</th>
            <th>Description</th>
            <th>Price</th>
            <th>5 Star Rating</th>
            <th></th>
          </tr>
        </thead>
        <tbody>{rows}</tbody>
      </table>
    ) : null;

    const deleteModal = (
      <Modal show={this.state.showDeleteModal} onHide={this.deleteCanceled}>
        <Modal.Header closeButton>
          <Modal.Title>Delete Product</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Are you sure you want to delete
          <strong> {this.state.deletingProduct?.name}</strong>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={this.deleteCanceled}>
            No
          </Button>
          <Button variant="primary" onClick={this.deleteConfirmed}>
            Yes
          </Button>
        </Modal.Footer>
      </Modal>
    );

    return (
      <div>
        <div className="card">
          <div className="card-header">
            {this.state.pageTitle}
            <NavLink
              className="btn btn-primary"
              style={{ float: "right" }}
              to="/products/add"
            >
              Add Product
            </NavLink>
          </div>
          <div className="card-body">
            <div className="row">
              <div className="col-md-2">Filter by:</div>
              <div className="col-md-4">
                <input
                  type="text"
                  value={this.state.listFilter}
                  onChange={event => this.filterChanged(event)}
                />
              </div>
            </div>
            {this.state.listFilter ? (
              <div className="row">
                <div className="col-md-6">
                  <h4>Filtered by: {this.state.listFilter}</h4>
                </div>
              </div>
            ) : null}
            <div className="table-responsive">{table}</div>
          </div>
        </div>
        {this.props.errorMessage ? (
          <div className="alert alert-danger">
            Error: {this.props.errorMessage}
          </div>
        ) : null}
        {deleteModal}
      </div>
    );
  }
}

const mapStateToProps = state => {
  return {
    products: state.product.products
  };
};

const mapDispatchToProps = dispatch => {
  return {
    fetchProducts: () => dispatch(actions.fetchProducts()),
    deleteProduct: product => dispatch(actions.deleteProduct(product))
  };
};

export default connect(mapStateToProps, mapDispatchToProps)(Products);
